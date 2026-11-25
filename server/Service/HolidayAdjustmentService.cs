using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayAdjustmentService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayBalanceService balances;
    private readonly HolidayLedgerService ledger;
    private readonly HolidayDebtService debt;
    private readonly HolidayConversionService conversions;

    public HolidayAdjustmentService(MyDatabaseConnection database, HolidayBalanceService balanceService,
        HolidayLedgerService ledgerService, HolidayDebtService debtService, HolidayConversionService conversionService)
    {
        db = database;
        balances = balanceService;
        ledger = ledgerService;
        debt = debtService;
        conversions = conversionService;
    }

    public HolidayBalanceAdjustment[] GetHistory(Guid userId)
    {
        return db.HolidayBalanceAdjustments.Where(adjustment => adjustment.UserId == userId)
            .OrderByDescending(adjustment => adjustment.CreatedAt).ToArray();
    }

    public HolidayBalanceAdjustment Create(Guid userId, HolidayAdjustmentRequest request, Guid createdBy)
    {
        CheckInput(request);
        // En transaktion gemmer hele rettelsen samlet eller slet ikke.
        using var transaction = db.BeginTransaction();
        // Samme lås som feriegodkendelse, så saldoen ikke ændres midt i en afgørelse.
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE",
            new DataParameter("id", userId)).ToArray();

        var existing = FindExistingAdjustment(userId, request);
        if (existing != null)
        {
            // Rettelsen er allerede gemt, så vi gemmer ikke en kopi.
            return existing;
        }

        if (request.Date != DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Nye saldorettelser skal bogføres med dagens dato.");
        }

        var day = GetOwnedAccrualDay(userId, request.AccrualDayId);
        if (request.Date > day.UsageEnd)
        {
            throw new ArgumentException("Afholdelsesperioden er udløbet.");
        }

        if (request.Hours < 0)
        {
            CheckRemainingHours(userId, day, request);
        }

        var adjustment = CreateAdjustmentRecord(userId, request, createdBy);
        adjustment.Account = day.Account;
        adjustment.EarningStart = day.EarningStart;
        adjustment.EarningEnd = day.EarningEnd;
        adjustment.UsageStart = day.UsageStart;
        adjustment.UsageEnd = day.UsageEnd;
        adjustment.DayLength = ledger.DayLengthOn(userId, request.Date);
        if (adjustment.DayLength <= 0)
        {
            throw new ArgumentException("Saldorettelsen kræver en positiv daglængde.");
        }

        // Insert tilføjer en ny række. De gamle poster bliver ikke ændret.
        db.Insert(adjustment);
        debt.ValidateExisting(userId);
        conversions.Record(userId);
        transaction.Commit();
        return adjustment;
    }

    public HolidayBalanceAdjustment CreateOpeningBalance(Guid userId, HolidayOpeningBalanceRequest request, Guid createdBy)
    {
        if (request.OperationId == Guid.Empty)
        {
            throw new ArgumentException("Angiv operations-ID.");
        }
        if (!Enum.IsDefined(request.Account))
        {
            throw new ArgumentException("Vælg en gyldig feriekonto.");
        }
        if (request.Hours <= 0)
        {
            throw new ArgumentException("Startsaldoen skal være positiv.");
        }
        if (request.Hours != Math.Round(request.Hours, 2))
        {
            throw new ArgumentException("Startsaldoen må højst have to decimaler.");
        }
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500)
        {
            throw new ArgumentException("Angiv en begrundelse på højst 500 tegn fra det gamle regnskab.");
        }
        if (request.EarningStart == default || request.UsageStart == default)
        {
            throw new ArgumentException("Angiv startdatoer for optjening og afholdelse.");
        }
        if (request.EarningEnd < request.EarningStart || request.UsageEnd < request.UsageStart)
        {
            throw new ArgumentException("Slutdatoen må ikke være før startdatoen.");
        }
        if (request.UsageStart < request.EarningStart)
        {
            throw new ArgumentException("Afholdelsesperioden må ikke starte før optjeningsperioden.");
        }

        using var transaction = db.BeginTransaction();
        HolidayLedgerService.LockRules(db);
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();

        var existing = db.HolidayBalanceAdjustments.SingleOrDefault(item => item.OperationId == request.OperationId);
        if (existing != null)
        {
            if (!HasSameOpeningBalance(existing, userId, request))
            {
                throw new InvalidOperationException("Operations-ID'et er allerede brugt til andet indhold.");
            }
            return existing;
        }

        var firstRule = db.HolidayRules.OrderBy(rule => rule.EffectiveFrom).FirstOrDefault();
        if (firstRule == null)
        {
            throw new ArgumentException("Opret ferieregler før startsaldoen registreres.");
        }
        var date = firstRule.EffectiveFrom;
        if (date > DateOnly.FromDateTime(DateTime.Today) || request.EarningStart >= date || request.UsageEnd < date)
        {
            throw new ArgumentException("Startsaldoen skal være optjent før systemets start og kunne afholdes efter starten.");
        }
        if (!db.Employments.Any(item => item.UserId == userId && item.Type != EmploymentType.Hourly &&
            item.StartDate < date && item.StartDate <= request.EarningEnd && (item.EndDate == null || item.EndDate >= date)))
        {
            throw new ArgumentException("Ansættelsen skal dække den importerede optjening og systemets start.");
        }
        bool alreadyImported = db.HolidayBalanceAdjustments.Any(item => item.UserId == userId && item.AccrualDayId == null &&
            item.Account == request.Account && item.EarningStart <= request.EarningEnd && item.EarningEnd >= request.EarningStart);
        if (alreadyImported)
        {
            throw new InvalidOperationException("Der er allerede registreret startsaldo for denne konto og optjeningsperiode.");
        }

        var adjustment = new HolidayBalanceAdjustment();
        adjustment.Id = Guid.NewGuid();
        adjustment.UserId = userId;
        adjustment.OperationId = request.OperationId;
        adjustment.Account = request.Account;
        adjustment.EarningStart = request.EarningStart;
        adjustment.EarningEnd = request.EarningEnd;
        adjustment.UsageStart = request.UsageStart;
        adjustment.UsageEnd = request.UsageEnd;
        adjustment.Date = date;
        adjustment.Hours = request.Hours;
        adjustment.DayLength = ledger.DayLengthOn(userId, date);
        if (adjustment.DayLength <= 0)
        {
            throw new ArgumentException("Startsaldoen kræver en arbejdsplan med arbejdstimer ved systemets start.");
        }
        adjustment.Reason = request.Reason.Trim();
        adjustment.CreatedAt = DateTime.UtcNow;
        adjustment.CreatedBy = createdBy;
        db.Insert(adjustment);
        debt.ValidateExisting(userId);
        conversions.Record(userId);
        transaction.Commit();
        return adjustment;
    }

    private static bool HasSameOpeningBalance(HolidayBalanceAdjustment existing,
        Guid userId, HolidayOpeningBalanceRequest request)
    {
        // Et gentaget kald skal have præcis det samme indhold.
        if (existing.UserId != userId) return false;
        if (existing.AccrualDayId != null) return false;
        if (existing.Account != request.Account) return false;
        if (existing.Hours != request.Hours) return false;
        if (existing.EarningStart != request.EarningStart) return false;
        if (existing.EarningEnd != request.EarningEnd) return false;
        if (existing.UsageStart != request.UsageStart) return false;
        if (existing.UsageEnd != request.UsageEnd) return false;
        if (existing.Reason != request.Reason.Trim()) return false;
        return true;
    }

    private static HolidayBalanceAdjustment CreateAdjustmentRecord(
        Guid userId, HolidayAdjustmentRequest request, Guid createdBy)
    {
        var adjustment = new HolidayBalanceAdjustment();
        adjustment.Id = Guid.NewGuid();
        adjustment.UserId = userId;
        adjustment.OperationId = request.OperationId;
        adjustment.AccrualDayId = request.AccrualDayId;
        adjustment.Date = request.Date;
        adjustment.Hours = request.Hours;
        adjustment.Reason = request.Reason.Trim();
        adjustment.CreatedAt = DateTime.UtcNow;
        adjustment.CreatedBy = createdBy;

        return adjustment;
    }

    private HolidayBalanceAdjustment? FindExistingAdjustment(Guid userId, HolidayAdjustmentRequest request)
    {
        // SingleOrDefault finder én post eller returnerer null, hvis den ikke findes.
        var existing = db.HolidayBalanceAdjustments
            .SingleOrDefault(adjustment => adjustment.OperationId == request.OperationId);

        if (existing == null)
        {
            return null;
        }

        if (!HasSameContent(existing, userId, request))
        {
            throw new InvalidOperationException("Operations-ID'et er allerede brugt til en anden rettelse.");
        }

        return existing;
    }

    private static bool HasSameContent(HolidayBalanceAdjustment existing,
        Guid userId, HolidayAdjustmentRequest request)
    {
        if (existing.UserId != userId)
        {
            return false;
        }

        if (existing.AccrualDayId != request.AccrualDayId)
        {
            return false;
        }

        if (existing.Date != request.Date)
        {
            return false;
        }

        if (existing.Hours != request.Hours)
        {
            return false;
        }

        // Trim fjerner mellemrum før og efter begrundelsen.
        if (existing.Reason != request.Reason.Trim())
        {
            return false;
        }

        return true;
    }

    private HolidayAccrualDay GetOwnedAccrualDay(Guid userId, Guid dayId)
    {
        var day = db.HolidayAccrualDays.SingleOrDefault(day => day.Id == dayId);
        if (day == null)
        {
            throw new ArgumentException("Optjeningsposten findes ikke.");
        }

        // Any undersøger, om optjeningspostens måned tilhører medarbejderen.
        bool belongsToUser = db.HolidayAccrualMonths.Any(month =>
            month.Id == day.AccrualMonthId && month.UserId == userId);
        if (!belongsToUser)
        {
            throw new ArgumentException("Optjeningsposten tilhører ikke medarbejderen.");
        }

        return day;
    }

    private void CheckRemainingHours(Guid userId, HolidayAccrualDay day, HolidayAdjustmentRequest request)
    {
        var balance = balances.Get(userId, request.Date);
        foreach (var period in balance.Periods)
        {
            if (!period.Matches(day))
            {
                continue;
            }

            decimal hoursAfterAdjustment = period.Hours + request.Hours;
            if (hoursAfterAdjustment < 0)
            {
                throw new InvalidOperationException("Rettelsen må ikke gøre periodens saldo negativ.");
            }

            return;
        }

        throw new ArgumentException("Der er ingen bogført optjening i den valgte periode.");
    }

    private static void CheckInput(HolidayAdjustmentRequest request)
    {
        // Guid.Empty er et tomt ID, som ikke må bruges her.
        if (request.OperationId == Guid.Empty)
        {
            throw new ArgumentException("Angiv operations-ID.");
        }

        if (request.AccrualDayId == Guid.Empty)
        {
            throw new ArgumentException("Angiv optjeningspostens ID.");
        }

        if (request.Hours == 0)
        {
            throw new ArgumentException("Timetallet må ikke være 0.");
        }

        decimal roundedHours = Math.Round(request.Hours, 2);
        if (roundedHours != request.Hours)
        {
            throw new ArgumentException("Timetallet må højst have to decimaler.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ArgumentException("Angiv en begrundelse.");
        }

        if (request.Reason.Length > 500)
        {
            throw new ArgumentException("Angiv en begrundelse på højst 500 tegn.");
        }
    }
}

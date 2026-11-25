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

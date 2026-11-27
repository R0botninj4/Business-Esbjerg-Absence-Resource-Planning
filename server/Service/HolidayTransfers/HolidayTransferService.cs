using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayTransferService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayAccrualService accrual;
    private readonly HolidayLedgerService ledger;
    private readonly HolidayDebtService debt;
    private readonly HolidayConversionService conversions;

    public HolidayTransferService(MyDatabaseConnection database, HolidayAccrualService accrualService,
        HolidayLedgerService ledgerService, HolidayDebtService debtService, HolidayConversionService conversionService)
    {
        db = database;
        accrual = accrualService;
        ledger = ledgerService;
        debt = debtService;
        conversions = conversionService;
    }

    public HolidayTransfer[] List(Guid userId, bool accounting)
    {
        if (accounting) return db.HolidayTransfers.OrderByDescending(item => item.CreatedAt).ToArray();
        return db.HolidayTransfers.Where(item => item.UserId == userId).OrderByDescending(item => item.CreatedAt).ToArray();
    }

    public HolidayTransfer? Get(Guid id, Guid userId, bool accounting)
    {
        var transfer = db.HolidayTransfers.SingleOrDefault(item => item.Id == id);
        if (transfer == null) return null;
        if (!accounting && transfer.UserId != userId) return null;
        return transfer;
    }

    public HolidayTransfer Create(Guid userId, HolidayTransferInput input)
    {
        if (input.OperationId == Guid.Empty || input.AccrualDayId == Guid.Empty)
            throw new ArgumentException("Angiv operations-ID og en bogført optjeningspost.");
        if (input.Hours <= 0 || input.Hours != Math.Round(input.Hours, 2))
            throw new ArgumentException("Angiv et positivt timetal med højst to decimaler.");

        accrual.PostMissingMonths(userId);
        using var transaction = db.BeginTransaction();
        HolidayLedgerService.LockRules(db);
        LockUser(userId);
        var existing = db.HolidayTransfers.SingleOrDefault(item => item.OperationId == input.OperationId);
        if (existing != null)
        {
            if (existing.UserId != userId || existing.AccrualDayId != input.AccrualDayId || existing.Hours != input.Hours)
                throw new HolidayRequestConflictException("Operations-ID'et er allerede brugt til andet indhold.");
            return existing;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rule = RuleForToday();
        // Find kun den valgte post her. Hele saldoen hentes ved valideringen nedenfor.
        var source = db.HolidayAccrualDays.SingleOrDefault(day => day.Id == input.AccrualDayId && day.Date <= today &&
            db.HolidayAccrualMonths.Any(month => month.Id == day.AccrualMonthId && month.UserId == userId));
        if (source == null) throw new ArgumentException("Vælg din egen bogførte optjening.");

        var transfer = new HolidayTransfer();
        transfer.Id = Guid.NewGuid();
        transfer.OperationId = input.OperationId;
        transfer.UserId = userId;
        transfer.AccrualDayId = source.Id;
        transfer.RuleId = rule.Id;
        transfer.Account = source.Account;
        transfer.EarningStart = source.EarningStart;
        transfer.EarningEnd = source.EarningEnd;
        transfer.SourceUsageStart = source.UsageStart;
        transfer.SourceUsageEnd = source.UsageEnd;
        transfer.TargetUsageStart = source.UsageEnd.AddDays(1);
        transfer.TargetUsageEnd = GetTargetEnd(transfer);
        if (transfer.TargetUsageEnd < transfer.TargetUsageStart)
            throw new ArgumentException("Den næste ferieperiode passer ikke med de gemte perioder.");
        transfer.Deadline = GetDeadline(transfer, rule);
        transfer.Hours = input.Hours;
        transfer.DayLength = ledger.DayLengthOn(userId, today);
        transfer.Days = HolidayLedgerService.ToDays(input.Hours, transfer.DayLength);
        transfer.CreatedAt = DateTime.UtcNow;
        ValidateAvailable(transfer, rule, today);
        db.Insert(transfer);
        transaction.Commit();
        return transfer;
    }

    public HolidayTransfer? Decide(Guid id, Guid decidedBy, HolidayTransferDecisionInput input)
    {
        string agreement = (input.AgreementReference ?? "").Trim();
        string comment = (input.Comment ?? "").Trim();
        if (agreement.Length > 500 || comment.Length > 500) throw new ArgumentException("Referencer og kommentarer må højst være 500 tegn.");
        if (input.Approve && agreement.Length == 0) throw new ArgumentException("Godkendelse kræver reference til en skriftlig aftale.");
        if (!input.Approve && comment.Length == 0) throw new ArgumentException("Skriv en begrundelse for afslaget.");

        using var transaction = db.BeginTransaction();
        HolidayLedgerService.LockRules(db);
        var transfer = db.HolidayTransfers.SingleOrDefault(item => item.Id == id);
        if (transfer == null) return null;
        LockUser(transfer.UserId);
        transfer = db.HolidayTransfers.Single(item => item.Id == id);
        string status = "declined";
        if (input.Approve) status = "approved";
        if (transfer.Status != "pending")
        {
            if (transfer.Status == status && transfer.DecidedBy == decidedBy && transfer.AgreementReference == agreement &&
                transfer.Comment == comment) return transfer;
            throw new HolidayRequestConflictException("Overførslen er allerede behandlet.");
        }
        if (input.Approve)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var rule = RuleForToday();
            if (ledger.DayLengthOn(transfer.UserId, today) != transfer.DayLength)
                throw new HolidayRequestConflictException("Arbejdstiden har ændret sig. Indsend en ny anmodning med det aktuelle timetal.");
            // En ny regel må gerne stramme fristen, men ikke forlænge en allerede gemt frist.
            var currentDeadline = GetDeadline(transfer, rule);
            if (today > currentDeadline) throw new HolidayRequestConflictException("Fristen for den skriftlige aftale er overskredet.");
            if (GetTargetEnd(transfer) != transfer.TargetUsageEnd)
                throw new HolidayRequestConflictException("Den næste ferieperiode har ændret sig. Indsend en ny anmodning.");
            ValidateAvailable(transfer, rule, today);
            transfer.RuleId = rule.Id;
            transfer.DebitDate = today;
            transfer.EffectiveDate = transfer.TargetUsageStart;
            if (transfer.EffectiveDate < today) transfer.EffectiveDate = today;
        }
        transfer.Status = status;
        transfer.AgreementReference = agreement;
        transfer.Comment = comment;
        transfer.DecidedAt = DateTime.UtcNow;
        transfer.DecidedBy = decidedBy;
        db.Update(transfer);
        if (input.Approve)
        {
            debt.ValidateExisting(transfer.UserId);
            conversions.Record(transfer.UserId);
        }
        transaction.Commit();
        return transfer;
    }

    private void ValidateAvailable(HolidayTransfer transfer, HolidayRule rule, DateOnly today)
    {
        if (!rule.AllowTransfer) throw new HolidayRequestConflictException("Virksomhedens regler tillader ikke ferieoverførsel.");
        if (!db.Users.Any(user => user.Id == transfer.UserId && user.Role == "Employee" && user.IsActive))
            throw new HolidayRequestConflictException("Medarbejderkontoen er ikke aktiv.");
        if (!db.Employments.Any(item => item.UserId == transfer.UserId && item.Type != EmploymentType.Hourly &&
            item.StartDate <= transfer.TargetUsageStart && (item.EndDate == null || item.EndDate >= transfer.TargetUsageStart)))
            throw new HolidayRequestConflictException("Overførslen kræver en ansættelse i den næste ferieperiode.");
        if (today < transfer.SourceUsageStart)
            throw new HolidayRequestConflictException("Kildeperiodens afholdelse er endnu ikke startet.");
        if (today > transfer.Deadline || today > transfer.TargetUsageEnd)
            throw new HolidayRequestConflictException("Overførselsfristen er overskredet.");
        var approved = db.HolidayTransfers.Where(item => item.UserId == transfer.UserId && item.Status == "approved").ToArray();
        // Loftet deles mellem kontiene for de nye afholdelsesperioder, der starter samme år.
        decimal movedDays = approved.Where(item => item.TargetUsageStart.Year == transfer.TargetUsageStart.Year).Sum(item => item.Days);
        if (movedDays + transfer.Days > rule.MaximumTransferDays + 0.00000001m)
            throw new HolidayRequestConflictException("Det samlede loft for overførte feriedage er overskredet.");

        var credits = ledger.PostedCredits(transfer.UserId, today);
        var balance = ledger.Calculate(transfer.UserId, today, credits, false);
        var source = balance.Periods.SingleOrDefault(period => period.Account == transfer.Account &&
            period.EarningStart == transfer.EarningStart && period.EarningEnd == transfer.EarningEnd &&
            period.UsageStart == transfer.SourceUsageStart && period.UsageEnd == transfer.SourceUsageEnd);
        if (source == null || source.Days - source.ReservedDays + 0.00000001m < transfer.Days)
            throw new HolidayRequestConflictException("Der er ikke nok ubrugt, bogført ferie i kildeperioden.");

        if (transfer.Account == HolidayAccount.WeeksOneToFive)
        {
            decimal earnedDays = 0;
            foreach (var day in credits.Where(day => source.Matches(day)))
                earnedDays += HolidayLedgerService.ToDays(day.Hours, ledger.DayLength(day.WorkScheduleId));
            var adjustments = db.HolidayBalanceAdjustments.Where(item => item.UserId == transfer.UserId && item.Date <= today &&
                item.Account == transfer.Account && item.EarningStart == transfer.EarningStart && item.EarningEnd == transfer.EarningEnd &&
                item.UsageStart == transfer.SourceUsageStart && item.UsageEnd == transfer.SourceUsageEnd).ToArray();
            foreach (var adjustment in adjustments) earnedDays += HolidayLedgerService.ToDays(adjustment.Hours, adjustment.DayLength);
            decimal alreadyMoved = approved.Where(item => item.Account == transfer.Account && item.EarningStart == transfer.EarningStart &&
                item.EarningEnd == transfer.EarningEnd).Sum(item => item.Days);
            decimal eligibleDays = earnedDays - rule.NonTransferableHolidayDays - alreadyMoved;
            if (transfer.Days > eligibleDays + 0.00000001m)
                throw new HolidayRequestConflictException("Kun ferie ud over de beskyttede uger kan overføres fra uge 1–5.");
        }
    }

    private HolidayRule RuleForToday()
    {
        var rule = HolidayRuleService.FindForDate(db, DateOnly.FromDateTime(DateTime.Today));
        if (rule?.Periods == null) throw new ArgumentException("Der mangler komplette ferieregler.");
        return rule;
    }

    private DateOnly GetTargetEnd(HolidayTransfer transfer)
    {
        var rule = HolidayRuleService.FindForDate(db, transfer.TargetUsageStart);
        if (rule?.Periods == null)
        {
            throw new ArgumentException("Der mangler regler for den næste ferieperiode.");
        }
        var periods = HolidayRulePeriodsCalculator.Calculate(rule.Periods, transfer.EarningStart.Year + 1);
        if (transfer.Account == HolidayAccount.SixthWeek)
        {
            return periods.SixthWeek.UsageEnd;
        }
        return periods.WeeksOneToFive.UsageEnd;
    }

    private static DateOnly GetDeadline(HolidayTransfer transfer, HolidayRule rule)
    {
        int month = rule.Periods!.HolidayTransferDeadlineMonth;
        int day = rule.Periods.HolidayTransferDeadlineDay;
        if (transfer.Account == HolidayAccount.SixthWeek)
        {
            month = rule.Periods.SixthWeekTransferDeadlineMonth;
            day = rule.Periods.SixthWeekTransferDeadlineDay;
        }
        var deadline = new DateOnly(transfer.SourceUsageEnd.Year, month, day);
        if (deadline < transfer.SourceUsageEnd) deadline = deadline.AddYears(1);
        return deadline;
    }

    private void LockUser(Guid userId)
    {
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
    }
}

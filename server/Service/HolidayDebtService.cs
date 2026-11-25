using Infra;
using LinqToDB;

namespace Service;

public class HolidayDebtService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayAccrualService accrual;
    private readonly HolidayLedgerService ledger;

    public HolidayDebtService(MyDatabaseConnection database, HolidayAccrualService accrualService,
        HolidayLedgerService ledgerService)
    {
        db = database;
        accrual = accrualService;
        ledger = ledgerService;
    }

    public List<HolidayUsage> Allocate(Guid userId, HolidayAccount account, List<HolidayRequestDay> requestedDays)
    {
        var history = HolidayRuleService.ReadHistory(db);
        var existingUsage = db.HolidayUsage.Where(item => item.UserId == userId).ToArray();
        var employments = db.Employments.Where(item => item.UserId == userId).ToArray();
        foreach (var usage in existingUsage)
        {
            if (!employments.Any(item => item.StartDate <= usage.Date && (item.EndDate == null || item.EndDate >= usage.Date)))
            {
                throw new HolidayRequestConflictException("En godkendt ferie ligger uden for ansættelsen. Afklar ferien før ansættelsen ændres.");
            }
        }
        var adjustments = db.HolidayBalanceAdjustments.Where(item => item.UserId == userId).ToArray();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var lastDate = today;
        foreach (var day in requestedDays)
        {
            if (day.Date > lastDate)
            {
                lastDate = day.Date;
            }
        }
        foreach (var item in existingUsage)
        {
            if (item.Date > lastDate)
            {
                lastDate = item.Date;
            }
        }
        var horizon = GetHorizon(lastDate, history);

        // Gå frem til gældsfristen med både eksisterende og ny godkendt ferie.
        var credits = accrual.Forecast(userId, horizon).Days;
        // Gruppér datoerne én gang. Så henter løkken kun posterne for den aktuelle dag.
        var creditDates = credits.ToLookup(day => day.Date);
        var usageDates = existingUsage.ToLookup(item => item.Date);
        var adjustmentDates = adjustments.ToLookup(item => item.Date);
        var requestDates = requestedDays.ToLookup(day => day.Date);
        var periods = new List<HolidayBalancePeriod>();
        var result = new List<HolidayUsage>();
        var date = today;
        if (credits.Count > 0)
        {
            date = credits.Min(day => day.Date);
        }
        if (existingUsage.Length > 0 && existingUsage.Min(item => item.Date) < date)
        {
            date = existingUsage.Min(item => item.Date);
        }
        if (adjustments.Length > 0 && adjustments.Min(item => item.Date) < date)
        {
            date = adjustments.Min(item => item.Date);
        }

        while (date <= horizon)
        {
            foreach (var credit in creditDates[date])
            {
                var period = HolidayLedgerService.ForAccrual(periods, credit);
                period.Days += HolidayLedgerService.ToDays(credit.Hours, ledger.DayLength(credit.WorkScheduleId));
            }
            foreach (var adjustment in adjustmentDates[date])
            {
                var period = HolidayLedgerService.ForAdjustment(periods, adjustment);
                period.Days += HolidayLedgerService.ToDays(adjustment.Hours, adjustment.DayLength);
            }
            foreach (var usage in usageDates[date])
            {
                HolidayLedgerService.ForUsage(periods, usage).Days -= usage.Days;
            }
            foreach (var day in requestDates[date])
            {
                AllocateDay(userId, account, day, periods, result, history);
            }
            CheckDebt(periods, date, history);
            if (date == horizon)
            {
                break;
            }
            date = date.AddDays(1);
        }
        return result;
    }

    private static DateOnly GetHorizon(DateOnly lastDate, HolidayRule[] history)
    {
        var lastRule = RuleOn(history, lastDate);
        var settings = lastRule.Periods!;
        var horizon = new DateOnly(lastDate.Year, settings.DebtDeadlineMonth, settings.DebtDeadlineDay);
        if (horizon < lastDate)
        {
            horizon = horizon.AddYears(1);
        }
        // En allerede planlagt regelændring kan flytte tilbagebetalingsfristen.
        bool extended = true;
        while (extended)
        {
            extended = false;
            foreach (var rule in history)
            {
                if (rule.EffectiveFrom > horizon || rule.Periods == null)
                {
                    continue;
                }
                var anchor = lastDate;
                if (rule.EffectiveFrom > lastDate)
                {
                    anchor = rule.EffectiveFrom;
                }
                if (RuleOn(history, anchor).Id != rule.Id)
                {
                    continue;
                }
                var deadline = new DateOnly(anchor.Year, rule.Periods.DebtDeadlineMonth, rule.Periods.DebtDeadlineDay);
                if (deadline < anchor)
                {
                    deadline = deadline.AddYears(1);
                }
                if (deadline > horizon)
                {
                    horizon = deadline;
                    extended = true;
                }
            }
        }

        return horizon;
    }

    public void ValidateExisting(Guid userId)
    {
        if (!db.HolidayUsage.Any(item => item.UserId == userId))
        {
            return;
        }
        Allocate(userId, HolidayAccount.WeeksOneToFive, new List<HolidayRequestDay>());
    }

    private void AllocateDay(Guid userId, HolidayAccount account, HolidayRequestDay day,
        List<HolidayBalancePeriod> periods, List<HolidayUsage> result, HolidayRule[] history)
    {
        if (day.Hours == 0)
        {
            return;
        }
        if (day.WorkScheduleId == null)
        {
            throw new ArgumentException("Der mangler en arbejdsplan for feriedagen.");
        }
        decimal length = ledger.DayLength(day.WorkScheduleId.Value);
        decimal remaining = day.Hours;
        // Brug først den gyldige periode, der udløber tidligst.
        foreach (var period in periods.OrderBy(period => period.UsageEnd).ToArray())
        {
            if (period.Account != account || day.Date < period.UsageStart || day.Date > period.UsageEnd || period.Days <= 0)
            {
                continue;
            }
            decimal coveredHours = Math.Floor(period.Days * length * 100 + 0.00000001m) / 100;
            decimal hours = Math.Min(remaining, coveredHours);
            if (hours > 0)
            {
                AddUsage(userId, day, period, hours, length, result);
                remaining -= hours;
            }
            if (remaining == 0)
            {
                return;
            }
        }

        // Resten er gæld i en gyldig periode. CheckDebt kontrollerer loft og tilbagebetaling.
        var debtPeriod = FindDebtPeriod(periods, account, day.Date, RuleOn(history, day.Date));
        AddUsage(userId, day, debtPeriod, remaining, length, result);
    }

    private static void AddUsage(Guid userId, HolidayRequestDay day, HolidayBalancePeriod period,
        decimal hours, decimal length, List<HolidayUsage> result)
    {
        decimal days = HolidayLedgerService.ToDays(hours, length);
        var usage = result.SingleOrDefault(item => item.RequestDayId == day.Id && item.Account == period.Account &&
            item.EarningStart == period.EarningStart && item.EarningEnd == period.EarningEnd &&
            item.UsageStart == period.UsageStart && item.UsageEnd == period.UsageEnd);
        if (usage == null)
        {
            usage = new HolidayUsage();
            usage.Id = Guid.NewGuid();
            usage.UserId = userId;
            usage.RequestDayId = day.Id;
            usage.Date = day.Date;
            usage.Account = period.Account;
            usage.EarningStart = period.EarningStart;
            usage.EarningEnd = period.EarningEnd;
            usage.UsageStart = period.UsageStart;
            usage.UsageEnd = period.UsageEnd;
            usage.DayLength = length;
            result.Add(usage);
        }
        usage.Hours += hours;
        usage.Days += days;
        period.Days -= days;
    }

    private static HolidayBalancePeriod FindDebtPeriod(List<HolidayBalancePeriod> periods,
        HolidayAccount account, DateOnly date, HolidayRule rule)
    {
        var settings = rule.Periods!;
        int startOffset = settings.HolidayUsageStartYearOffset;
        int endOffset = settings.HolidayUsageEndYearOffset;
        if (account == HolidayAccount.SixthWeek)
        {
            startOffset = settings.SixthWeekUsageStartYearOffset;
            endOffset = settings.SixthWeekUsageEndYearOffset;
        }
        for (int year = date.Year - startOffset; year >= Math.Max(1, date.Year - endOffset); year--)
        {
            var calculated = HolidayRulePeriodsCalculator.Calculate(settings, year);
            var period = calculated.WeeksOneToFive;
            if (account == HolidayAccount.SixthWeek)
            {
                period = calculated.SixthWeek;
            }
            if (period.UsageStart <= date && period.UsageEnd >= date)
            {
                return HolidayLedgerService.GetPeriod(periods, account, period.EarningStart, period.EarningEnd, period.UsageStart, period.UsageEnd);
            }
        }
        throw new HolidayRequestConflictException("Feriens konto har ingen gyldig afholdelsesperiode.");
    }

    private static void CheckDebt(List<HolidayBalancePeriod> periods, DateOnly date, HolidayRule[] history)
    {
        var negative = periods.Where(period => period.Days < -0.00000001m).ToArray();
        if (negative.Length == 0)
        {
            return;
        }
        var rule = RuleOn(history, date);
        decimal debt = negative.Sum(period => -period.Days);
        if (debt > rule.MaximumDebtDays + 0.00000001m)
        {
            throw new HolidayRequestConflictException("Feriegælden overstiger grænsen på " + rule.MaximumDebtDays + " dage den " + date + ".");
        }
        bool deadline = date.Month == rule.Periods!.DebtDeadlineMonth && date.Day == rule.Periods.DebtDeadlineDay;
        if (deadline || negative.Any(period => period.UsageEnd <= date))
        {
            throw new HolidayRequestConflictException("Feriegælden skal være betalt ved gældsfristen og inden afholdelsesperiodens udløb. Kontrollen fejlede den " + date + ".");
        }
    }

    private static HolidayRule RuleOn(HolidayRule[] history, DateOnly date)
    {
        var rule = history.FirstOrDefault(rule => rule.EffectiveFrom <= date);
        if (rule == null || rule.Periods == null)
        {
            throw new ArgumentException("Der mangler komplette ferieregler for " + date + ".");
        }
        return rule;
    }
}

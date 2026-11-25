using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

// Timer gemmes uændret. Dagværdien gør, at restferien overlever et skift i arbejdstid.
public class HolidayLedgerService
{
    private readonly MyDatabaseConnection db;
    private readonly Dictionary<Guid, decimal> dayLengths = new Dictionary<Guid, decimal>();

    public HolidayLedgerService(MyDatabaseConnection database)
    {
        db = database;
    }

    public static void LockRules(MyDatabaseConnection database)
    {
        // PostgreSQL deler denne lås mellem regelændringer og godkendelser.
        database.Execute("SELECT pg_advisory_xact_lock(71462)");
    }

    public decimal DayLength(Guid scheduleId)
    {
        // Samme plan bruges ofte. Genbrug dens daglængde inden for dette API-kald.
        if (dayLengths.TryGetValue(scheduleId, out var saved))
        {
            return saved;
        }
        decimal hours = 0;
        var days = db.WorkScheduleDays.Where(day => day.WorkScheduleId == scheduleId && !day.IsDayOff).ToArray();
        foreach (var day in days)
        {
            if (day.StartTime == null || day.EndTime == null)
            {
                throw new ArgumentException("Arbejdsplanen mangler klokkeslæt.");
            }
            hours += (decimal)(day.EndTime.Value - day.StartTime.Value).TotalHours;
        }
        // Flere decimaler internt bevarer dagværdien. Svaret afrundes til to decimaler.
        decimal length = Math.Round(hours / 5, 12);
        dayLengths[scheduleId] = length;
        return length;
    }

    public WorkSchedule[] Plans(Guid userId)
    {
        var employmentIds = db.Employments.Where(employment => employment.UserId == userId)
            .Select(employment => employment.Id).ToArray();
        return db.WorkSchedules.Where(plan => employmentIds.Contains(plan.EmploymentId))
            .OrderBy(plan => plan.StartDate).ToArray();
    }

    public decimal DayLengthOn(Guid userId, DateOnly date)
    {
        // Efter fratræden beholdes den sidst kendte daglængde til restsaldoen.
        var plan = Plans(userId).LastOrDefault(plan => plan.StartDate <= date);
        if (plan == null)
        {
            return 0;
        }
        return DayLength(plan.Id);
    }

    public static decimal ToDays(decimal hours, decimal dayLength)
    {
        if (hours == 0)
        {
            return 0;
        }
        if (dayLength <= 0)
        {
            throw new ArgumentException("Timer kan ikke omregnes uden en positiv daglængde.");
        }
        return Math.Round(hours / dayLength, 12);
    }

    public static HolidayBalancePeriod GetPeriod(List<HolidayBalancePeriod> periods, HolidayAccount account,
        DateOnly earningStart, DateOnly earningEnd, DateOnly usageStart, DateOnly usageEnd)
    {
        foreach (var period in periods)
        {
            if (period.Account == account && period.EarningStart == earningStart && period.EarningEnd == earningEnd &&
                period.UsageStart == usageStart && period.UsageEnd == usageEnd)
            {
                return period;
            }
        }
        var added = new HolidayBalancePeriod();
        added.Account = account;
        added.EarningStart = earningStart;
        added.EarningEnd = earningEnd;
        added.UsageStart = usageStart;
        added.UsageEnd = usageEnd;
        periods.Add(added);
        return added;
    }

    public static HolidayBalancePeriod ForAccrual(List<HolidayBalancePeriod> periods, HolidayAccrualDay day)
    {
        return GetPeriod(periods, day.Account, day.EarningStart, day.EarningEnd, day.UsageStart, day.UsageEnd);
    }

    public static HolidayBalancePeriod ForUsage(List<HolidayBalancePeriod> periods, HolidayUsage usage)
    {
        return GetPeriod(periods, usage.Account, usage.EarningStart, usage.EarningEnd, usage.UsageStart, usage.UsageEnd);
    }

    public HolidayBalanceResult Calculate(Guid userId, DateOnly date, HolidayAccrualDay[] credits, bool forecast,
        HolidayUsage[]? selectedUsage = null)
    {
        var result = new HolidayBalanceResult();
        result.Date = date;
        result.IncludesExpectedAccrual = forecast;
        result.IncludesHolidayUsage = true;
        result.IncludesScheduleConversions = true;
        decimal length = DayLengthOn(userId, date);

        foreach (var day in credits)
        {
            var period = ForAccrual(result.Periods, day);
            if (day.AccrualMonthId == Guid.Empty)
            {
                period.ExpectedHours += day.Hours;
            }
            else
            {
                period.PostedHours += day.Hours;
            }
            period.Days += ToDays(day.Hours, DayLength(day.WorkScheduleId));
        }

        var adjustments = db.HolidayBalanceAdjustments.Where(item => item.UserId == userId && item.Date <= date).ToArray();
        foreach (var item in adjustments)
        {
            var source = db.HolidayAccrualDays.Single(day => day.Id == item.AccrualDayId);
            var period = ForAccrual(result.Periods, source);
            period.AdjustmentHours += item.Hours;
            period.Days += ToDays(item.Hours, item.DayLength);
        }

        var usage = selectedUsage ?? db.HolidayUsage.Where(item => item.UserId == userId).ToArray();
        foreach (var item in usage)
        {
            var period = ForUsage(result.Periods, item);
            if (item.Date <= date)
            {
                period.UsedHours += item.Hours;
                period.Days -= item.Days;
            }
            else
            {
                period.ReservedHours += item.Hours;
                period.ReservedDays += item.Days;
            }
        }

        foreach (var period in result.Periods)
        {
            period.DayLength = length;
            period.Hours = Math.Round((period.Days - period.ReservedDays) * length, 2);
            period.ConversionHours = period.Hours - (period.PostedHours + period.ExpectedHours + period.AdjustmentHours - period.UsedHours - period.ReservedHours);
            period.DebtHours = Math.Round(Math.Max(0, -period.Days) * length, 2);
            if (date < period.UsageStart || date > period.UsageEnd)
            {
                continue;
            }
            if (period.Account == HolidayAccount.WeeksOneToFive)
            {
                result.WeeksOneToFiveHours += period.Hours;
                result.WeeksOneToFiveDebtHours += period.DebtHours;
            }
            else
            {
                result.SixthWeekHours += period.Hours;
                result.SixthWeekDebtHours += period.DebtHours;
            }
        }
        return result;
    }

    public HolidayAccrualDay[] PostedCredits(Guid userId, DateOnly date)
    {
        var monthIds = db.HolidayAccrualMonths.Where(month => month.UserId == userId).Select(month => month.Id).ToArray();
        return db.HolidayAccrualDays.Where(day => monthIds.Contains(day.AccrualMonthId) && day.Date <= date)
            .OrderBy(day => day.Date).ToArray();
    }
}

using Infra;
using LinqToDB;

namespace Service;

public class HolidayBalanceService
{
    private readonly MyDatabaseConnection db;

    public HolidayBalanceService(MyDatabaseConnection database)
    {
        db = database;
    }

    public HolidayBalanceResult Get(Guid userId, DateOnly date)
    {
        var result = new HolidayBalanceResult();
        result.Date = date;

        // 1. Læg optjeningen sammen. 2. Tilføj rettelser. 3. Beregn de to konti.
        AddPostedAccrual(userId, date, result);
        AddAdjustments(userId, date, result);
        AddUsage(userId, date, result);
        result.IncludesHolidayUsage = true;
        CalculateTotals(date, result);

        return result;
    }

    private void AddPostedAccrual(Guid userId, DateOnly date, HolidayBalanceResult result)
    {
        // Where vælger medarbejderens måneder. ToArray henter dem fra databasen.
        var months = db.HolidayAccrualMonths
            .Where(month => month.UserId == userId).ToArray();

        foreach (var month in months)
        {
            var days = db.HolidayAccrualDays
                .Where(day => day.AccrualMonthId == month.Id && day.Date <= date).ToArray();

            foreach (var day in days)
            {
                var period = FindPeriod(result, day);
                if (period == null)
                {
                    period = CreatePeriod(day);
                    result.Periods.Add(period);
                }

                period.PostedHours = period.PostedHours + day.Hours;
            }
        }
    }

    private static void CalculateTotals(DateOnly date, HolidayBalanceResult result)
    {
        foreach (var period in result.Periods)
        {
            period.PostedHours = Math.Round(period.PostedHours, 2);
            period.AdjustmentHours = Math.Round(period.AdjustmentHours, 2);
            period.Hours = period.PostedHours + period.AdjustmentHours - period.UsedHours - period.ReservedHours;

            // Fremtidige og udløbne perioder vises, men tæller ikke i den aktuelle sum.
            if (date < period.UsageStart)
            {
                continue;
            }

            if (date > period.UsageEnd)
            {
                continue;
            }

            if (period.Account == HolidayAccount.WeeksOneToFive)
            {
                result.WeeksOneToFiveHours = result.WeeksOneToFiveHours + period.Hours;
            }
            else if (period.Account == HolidayAccount.SixthWeek)
            {
                result.SixthWeekHours = result.SixthWeekHours + period.Hours;
            }
        }
    }

    private static HolidayBalancePeriod CreatePeriod(HolidayAccrualDay day)
    {
        var period = new HolidayBalancePeriod();
        period.Account = day.Account;
        period.EarningStart = day.EarningStart;
        period.EarningEnd = day.EarningEnd;
        period.UsageStart = day.UsageStart;
        period.UsageEnd = day.UsageEnd;
        return period;
    }

    private void AddUsage(Guid userId, DateOnly date, HolidayBalanceResult result)
    {
        var usage = db.HolidayUsage.Where(item => item.UserId == userId).ToArray();
        foreach (var item in usage)
        {
            var source = db.HolidayAccrualDays.Single(day => day.Id == item.AccrualDayId);
            var period = FindPeriod(result, source);
            if (period == null)
            {
                period = CreatePeriod(source);
                result.Periods.Add(period);
            }
            if (item.Date <= date)
            {
                period.UsedHours += item.Hours;
            }
            else
            {
                period.ReservedHours += item.Hours;
            }
        }
    }

    private void AddAdjustments(Guid userId, DateOnly date, HolidayBalanceResult result)
    {
        var adjustments = db.HolidayBalanceAdjustments
            .Where(adjustment => adjustment.UserId == userId && adjustment.Date <= date).ToArray();

        foreach (var adjustment in adjustments)
        {
            var day = db.HolidayAccrualDays.SingleOrDefault(day => day.Id == adjustment.AccrualDayId);
            if (day == null)
            {
                throw new InvalidOperationException("En saldorettelse mangler sit optjeningsgrundlag.");
            }

            var period = FindPeriod(result, day);
            if (period == null)
            {
                throw new InvalidOperationException("En saldorettelse mangler sin saldo-periode.");
            }

            period.AdjustmentHours = period.AdjustmentHours + adjustment.Hours;
        }
    }

    private static HolidayBalancePeriod? FindPeriod(HolidayBalanceResult result, HolidayAccrualDay day)
    {
        foreach (var period in result.Periods)
        {
            if (period.Matches(day))
            {
                return period;
            }
        }

        return null;
    }
}

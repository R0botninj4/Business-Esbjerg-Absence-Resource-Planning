using Infra;

namespace Service;

public static class HolidayRulePeriodsCalculator
{
    public static HolidayRulePeriodsResult Calculate(HolidayRulePeriodSettings settings, int earningYear)
    {
        var result = new HolidayRulePeriodsResult();
        var holiday = new HolidayAccountPeriod();
        holiday.EarningStart = GetDate(earningYear, 0,
            settings.HolidayEarningStartMonth, settings.HolidayEarningStartDay);
        holiday.EarningEnd = holiday.EarningStart.AddYears(1).AddDays(-1);
        holiday.UsageStart = GetDate(earningYear, settings.HolidayUsageStartYearOffset,
            settings.HolidayUsageStartMonth, settings.HolidayUsageStartDay);
        holiday.UsageEnd = GetDate(earningYear, settings.HolidayUsageEndYearOffset,
            settings.HolidayUsageEndMonth, settings.HolidayUsageEndDay);
        holiday.TransferDeadline = NextDate(holiday.UsageEnd,
            settings.HolidayTransferDeadlineMonth, settings.HolidayTransferDeadlineDay);
        ValidatePeriod(holiday);
        result.WeeksOneToFive = holiday;

        var sixthWeek = new HolidayAccountPeriod();
        sixthWeek.EarningStart = GetDate(earningYear, 0,
            settings.SixthWeekEarningStartMonth, settings.SixthWeekEarningStartDay);
        sixthWeek.EarningEnd = sixthWeek.EarningStart.AddYears(1).AddDays(-1);
        sixthWeek.UsageStart = GetDate(earningYear, settings.SixthWeekUsageStartYearOffset,
            settings.SixthWeekUsageStartMonth, settings.SixthWeekUsageStartDay);
        sixthWeek.UsageEnd = GetDate(earningYear, settings.SixthWeekUsageEndYearOffset,
            settings.SixthWeekUsageEndMonth, settings.SixthWeekUsageEndDay);
        sixthWeek.TransferDeadline = NextDate(sixthWeek.UsageEnd,
            settings.SixthWeekTransferDeadlineMonth, settings.SixthWeekTransferDeadlineDay);
        ValidatePeriod(sixthWeek);
        result.SixthWeek = sixthWeek;

        result.MainHolidayStart = NextDate(holiday.EarningStart,
            settings.MainHolidayStartMonth, settings.MainHolidayStartDay);
        result.MainHolidayEnd = NextDate(result.MainHolidayStart,
            settings.MainHolidayEndMonth, settings.MainHolidayEndDay);
        result.DebtDeadline = NextDate(holiday.EarningStart,
            settings.DebtDeadlineMonth, settings.DebtDeadlineDay);

        if (settings.MainHolidayDays < 0 ||
            settings.MainHolidayNoticeMonths < 0 || settings.OtherHolidayNoticeMonths < 0)
        {
            throw new ArgumentException("Hovedferiedage og varsler må ikke være negative.");
        }

        int windowDays = result.MainHolidayEnd.DayNumber - result.MainHolidayStart.DayNumber + 1;
        if (settings.MainHolidayDays > windowDays)
        {
            throw new ArgumentException("Hovedferien kan ikke være længere end hele datovinduet.");
        }

        return result;
    }

    private static DateOnly GetDate(int year, int yearOffset, int month, int day)
    {
        if (year < 1 || year > 9998 || yearOffset < 0 || yearOffset > 9998 - year)
        {
            throw new ArgumentException("Året og årsforskydningen giver en ugyldig dato.");
        }

        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Måneden skal være mellem 1 og 12.");
        }

        // Datoen skal kunne bruges hvert år, også når det ikke er skudår.
        if (day < 1 || day > DateTime.DaysInMonth(2001, month))
        {
            throw new ArgumentException("Dagen findes ikke i den valgte måned hvert år.");
        }

        return new DateOnly(year + yearOffset, month, day);
    }

    private static DateOnly NextDate(DateOnly start, int month, int day)
    {
        var date = GetDate(start.Year, 0, month, day);
        if (date < start)
        {
            date = date.AddYears(1);
        }

        return date;
    }

    private static void ValidatePeriod(HolidayAccountPeriod period)
    {
        if (period.UsageStart < period.EarningStart ||
            period.UsageEnd < period.UsageStart || period.UsageEnd < period.EarningEnd)
        {
            throw new ArgumentException("Afholdelsesperioden passer ikke med optjeningsperioden.");
        }
    }
}

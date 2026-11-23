namespace Service;

public class HolidayRulePeriodsResult
{
    public HolidayAccountPeriod WeeksOneToFive { get; set; } = new HolidayAccountPeriod();

    public HolidayAccountPeriod SixthWeek { get; set; } = new HolidayAccountPeriod();

    public bool SixthWeekEnabled { get; set; }

    public DateOnly MainHolidayStart { get; set; }

    public DateOnly MainHolidayEnd { get; set; }

    public DateOnly DebtDeadline { get; set; }
}

public class HolidayAccountPeriod
{
    public DateOnly EarningStart { get; set; }

    public DateOnly EarningEnd { get; set; }

    public DateOnly UsageStart { get; set; }

    public DateOnly UsageEnd { get; set; }

    public DateOnly TransferDeadline { get; set; }
}

using Infra;

namespace Service;

public class HolidayAccrualResult
{
    public decimal WeeksOneToFiveHours { get; set; }

    public decimal SixthWeekHours { get; set; }

    public bool IsPosted { get; set; }

    public List<HolidayAccrualDay> Days { get; set; } = new List<HolidayAccrualDay>();
}

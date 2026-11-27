using Infra;

namespace Service;

public static class HolidayHoursCalculator
{
    public static decimal CalculateDay(WorkScheduleDay day)
    {
        if (day.Day == DayOfWeek.Saturday || day.Day == DayOfWeek.Sunday)
        {
            return 0;
        }

        if (day.IsDayOff)
        {
            return 0;
        }

        if (!day.StartTime.HasValue || !day.EndTime.HasValue)
        {
            throw new ArgumentException("Arbejdsdagen mangler start- eller sluttid.");
        }

        if (day.EndTime.Value <= day.StartTime.Value)
        {
            throw new ArgumentException("Sluttiden skal være efter starttiden.");
        }

        var duration = day.EndTime.Value - day.StartTime.Value;
        var hours = (decimal)duration.TotalHours;

        return Math.Round(hours, 2);
    }
}

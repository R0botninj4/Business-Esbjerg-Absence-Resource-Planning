namespace Service;

public class WorkScheduleDayRequest
{
    public DayOfWeek Day { get; set; }

    public bool IsDayOff { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }
}
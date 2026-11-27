namespace Service;

public class HolidayHoursResult
{
    public decimal Hours { get; set; }

    public List<HolidayHoursDay> Days { get; set; } = new List<HolidayHoursDay>();
}

public class HolidayHoursDay
{
    public DateOnly Date { get; set; }

    public decimal Hours { get; set; }

    public string Description { get; set; } = "";

    public Guid? WorkScheduleId { get; set; }

    public Guid? CalendarDayId { get; set; }
}

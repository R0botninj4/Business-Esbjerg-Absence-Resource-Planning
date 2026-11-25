namespace Service;

public class ColleagueCalendarResult
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<ColleagueCalendarEmployee> Employees { get; set; } = new();
    public List<ColleagueCalendarHoliday> Holidays { get; set; } = new();
    public List<ColleagueCalendarDay> Days { get; set; } = new();
}

public class ColleagueCalendarEmployee
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public class ColleagueCalendarHoliday
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class ColleagueCalendarDay
{
    // Planlagt tilstedeværelse ud fra ugeplaner og godkendt ferie, ikke faktisk fremmøde.
    public DateOnly Date { get; set; }
    public int ScheduledCount { get; set; }
    public int HolidayCount { get; set; }
    public int PresentCount { get; set; }
    public int MissingScheduleCount { get; set; }
    public List<ColleagueCalendarCell> Employees { get; set; } = new();
}

public class ColleagueCalendarCell
{
    public Guid EmployeeId { get; set; }
    // working, holiday, dayOff, notEmployed eller missingSchedule.
    public string Status { get; set; } = "";
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}

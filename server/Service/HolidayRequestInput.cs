using Infra;

namespace Service;

public class HolidayRequestInput
{
    public HolidayAccount Account { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class HolidayRequestPreview
{
    public decimal Hours { get; set; }
    public List<HolidayRequestDay> Days { get; set; } = new List<HolidayRequestDay>();
}

public class HolidayRequestConflictException : Exception
{
    public HolidayRequestConflictException(string message) : base(message)
    {
    }
}

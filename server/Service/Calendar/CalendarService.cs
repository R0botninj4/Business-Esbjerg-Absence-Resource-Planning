using Infra;
using LinqToDB;

namespace Service;

public class CalendarService
{
    private readonly MyDatabaseConnection db;

    public CalendarService(MyDatabaseConnection database)
    {
        db = database;
    }

    public CalendarDay[] GetHistory()
    {
        return db.CalendarDays
            .OrderBy(day => day.Date)
            .ThenByDescending(day => day.Version)
            .ToArray();
    }

    public CalendarDay? GetForDate(DateOnly date)
    {
        return db.CalendarDays
            .Where(day => day.Date == date)
            .OrderByDescending(day => day.Version)
            .FirstOrDefault();
    }

    public CalendarDay Create(
        DateOnly date, string name, bool usesHolidayHours, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Angiv et navn på dagen.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date <= today)
        {
            throw new ArgumentException("Kalenderændringer skal gælde en fremtidig dato.");
        }

        using var transaction = db.BeginTransaction();
        var previousDay = GetForDate(date);
        int version = 1;

        if (previousDay != null)
        {
            version = previousDay.Version + 1;
        }

        var day = new CalendarDay();
        day.Id = Guid.NewGuid();
        day.Date = date;
        day.Name = name.Trim();
        day.UsesHolidayHours = usesHolidayHours;
        day.Version = version;
        day.CreatedAt = DateTime.UtcNow;
        day.CreatedBy = createdBy;

        // En ændring er en ny række. Den gamle version bliver liggende.
        db.Insert(day);
        transaction.Commit();

        return day;
    }
}

using LinqToDB.Mapping;

namespace Infra;

[Table("CalendarDays")]
public class CalendarDay
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public DateOnly Date { get; set; }

    [Column, NotNull]
    public string Name { get; set; } = "";

    [Column, NotNull]
    // False betyder, at dagen ikke bruger ferietimer.
    public bool UsesHolidayHours { get; set; }

    [Column, NotNull]
    public int Version { get; set; }

    [Column, NotNull]
    public DateTime CreatedAt { get; set; }

    [Column, NotNull]
    public Guid CreatedBy { get; set; }
}

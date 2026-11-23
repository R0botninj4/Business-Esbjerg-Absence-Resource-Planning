using LinqToDB.Mapping;

namespace Infra;

[Table("calendar_days")]
public class CalendarDay
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("date"), NotNull]
    public DateOnly Date { get; set; }

    [Column("name"), NotNull]
    public string Name { get; set; } = "";

    [Column("uses_holiday_hours"), NotNull]
    public bool UsesHolidayHours { get; set; }

    [Column("version"), NotNull]
    public int Version { get; set; }

    [Column("created_at"), NotNull]
    public DateTime CreatedAt { get; set; }

    [Column("created_by"), NotNull]
    public Guid CreatedBy { get; set; }
}
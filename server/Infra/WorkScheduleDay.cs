using LinqToDB.Mapping;

namespace Infra;

[Table("WorkScheduleDays")]
public class WorkScheduleDay
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    // Forbinder dagen med dens ugeplan.
    public Guid WorkScheduleId { get; set; }

    [Column, NotNull]
    public DayOfWeek Day { get; set; }

    [Column, NotNull]
    public bool IsDayOff { get; set; }

    [Column, Nullable]
    // På en fridag er start- og sluttid tomme.
    public TimeOnly? StartTime { get; set; }

    [Column, Nullable]
    public TimeOnly? EndTime { get; set; }
}

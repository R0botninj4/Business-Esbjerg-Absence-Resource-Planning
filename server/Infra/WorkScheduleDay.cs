using LinqToDB.Mapping;

namespace Infra;

[Table("work_schedule_days")]
public class WorkScheduleDay
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("work_schedule_id"), NotNull]
    public Guid WorkScheduleId { get; set; }

    [Column("day"), NotNull]
    public DayOfWeek Day { get; set; }

    [Column("is_day_off"), NotNull]
    public bool IsDayOff { get; set; }

    [Column("start_time"), Nullable]
    public TimeOnly? StartTime { get; set; }

    [Column("end_time"), Nullable]
    public TimeOnly? EndTime { get; set; }
}
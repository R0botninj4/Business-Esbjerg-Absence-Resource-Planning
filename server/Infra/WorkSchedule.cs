using LinqToDB.Mapping;

namespace Infra;

[Table("work_schedules")]
public class WorkSchedule
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("employment_id"), NotNull]
    public Guid EmploymentId { get; set; }

    [Column("start_date"), NotNull]
    public DateOnly StartDate { get; set; }

    [Column("end_date"), Nullable]
    public DateOnly? EndDate { get; set; }
}
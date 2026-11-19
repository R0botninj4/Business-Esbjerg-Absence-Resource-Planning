using LinqToDB.Mapping;

namespace Infra;

[Table("WorkSchedules")]
public class WorkSchedule
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    // Ansættelsen, som ugeplanen tilhører.
    public Guid EmploymentId { get; set; }

    [Column, NotNull]
    public DateOnly StartDate { get; set; }

    [Column, Nullable]
    // Tom slutdato betyder, at planen fortsætter.
    public DateOnly? EndDate { get; set; }
}

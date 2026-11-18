using LinqToDB.Mapping;

namespace Infra;

[Table("Employments")]
public class Employment
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public Guid UserId { get; set; }

    [Column, NotNull]
    public EmploymentType Type { get; set; }

    [Column, NotNull]
    public DateOnly StartDate { get; set; }

    [Column, Nullable]
    public DateOnly? EndDate { get; set; }
}
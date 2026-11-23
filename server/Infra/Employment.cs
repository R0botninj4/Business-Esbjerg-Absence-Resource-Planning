using LinqToDB.Mapping;

namespace Infra;

[Table("employments")]
public class Employment
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("user_id"), NotNull]
    public Guid UserId { get; set; }

    [Column("type"), NotNull]
    public EmploymentType Type { get; set; }

    [Column("start_date"), NotNull]
    public DateOnly StartDate { get; set; }

    [Column("end_date"), Nullable]
    public DateOnly? EndDate { get; set; }
}
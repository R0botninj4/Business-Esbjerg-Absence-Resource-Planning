using LinqToDB.Mapping;

namespace Infra;

[Table("HolidayAccrualMonths")]
public class HolidayAccrualMonth
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public Guid UserId { get; set; }

    [Column, NotNull]
    public DateOnly Month { get; set; }

    [Column, NotNull]
    public DateTime PostedAt { get; set; }

    [Column, NotNull]
    public Guid PostedBy { get; set; }
}

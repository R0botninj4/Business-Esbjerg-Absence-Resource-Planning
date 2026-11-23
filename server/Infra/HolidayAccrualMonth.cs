using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_accrual_months")]
public class HolidayAccrualMonth
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("user_id"), NotNull]
    public Guid UserId { get; set; }

    [Column("month"), NotNull]
    public DateOnly Month { get; set; }

    [Column("posted_at"), NotNull]
    public DateTime PostedAt { get; set; }

    [Column("posted_by"), NotNull]
    public Guid PostedBy { get; set; }
}
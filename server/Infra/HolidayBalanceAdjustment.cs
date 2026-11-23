using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_balance_adjustments")]
public class HolidayBalanceAdjustment
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("user_id"), NotNull]
    public Guid UserId { get; set; }

    [Column("operation_id"), NotNull]
    public Guid OperationId { get; set; }

    [Column("accrual_day_id"), NotNull]
    public Guid AccrualDayId { get; set; }

    [Column("date"), NotNull]
    public DateOnly Date { get; set; }

    [Column("hours"), NotNull]
    public decimal Hours { get; set; }

    [Column("reason"), NotNull]
    public string Reason { get; set; } = "";

    [Column("created_at"), NotNull]
    public DateTime CreatedAt { get; set; }

    [Column("created_by"), NotNull]
    public Guid CreatedBy { get; set; }
}

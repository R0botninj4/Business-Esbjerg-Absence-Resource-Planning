using LinqToDB.Mapping;

namespace Infra;

[Table("HolidayBalanceAdjustments")]
public class HolidayBalanceAdjustment
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public Guid UserId { get; set; }

    [Column, NotNull]
    public Guid OperationId { get; set; }

    [Column, NotNull]
    public Guid AccrualDayId { get; set; }

    [Column, NotNull]
    public DateOnly Date { get; set; }

    [Column, NotNull]
    public decimal Hours { get; set; }

    [Column, NotNull]
    public string Reason { get; set; } = "";

    [Column, NotNull]
    public DateTime CreatedAt { get; set; }

    [Column, NotNull]
    public Guid CreatedBy { get; set; }
}

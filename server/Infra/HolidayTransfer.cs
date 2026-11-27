using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_transfers")]
public class HolidayTransfer
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("operation_id")] public Guid OperationId { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("accrual_day_id")] public Guid AccrualDayId { get; set; }
    [Column("rule_id")] public Guid RuleId { get; set; }
    [Column("account")] public HolidayAccount Account { get; set; }
    [Column("earning_start")] public DateOnly EarningStart { get; set; }
    [Column("earning_end")] public DateOnly EarningEnd { get; set; }
    [Column("source_usage_start")] public DateOnly SourceUsageStart { get; set; }
    [Column("source_usage_end")] public DateOnly SourceUsageEnd { get; set; }
    [Column("target_usage_start")] public DateOnly TargetUsageStart { get; set; }
    [Column("target_usage_end")] public DateOnly TargetUsageEnd { get; set; }
    [Column("deadline")] public DateOnly Deadline { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("days")] public decimal Days { get; set; }
    [Column("day_length")] public decimal DayLength { get; set; }
    [Column("debit_date"), Nullable] public DateOnly? DebitDate { get; set; }
    [Column("effective_date"), Nullable] public DateOnly? EffectiveDate { get; set; }
    [Column("status")] public string Status { get; set; } = "pending";
    [Column("agreement_reference")] public string AgreementReference { get; set; } = "";
    [Column("comment")] public string Comment { get; set; } = "";
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("decided_at"), Nullable] public DateTime? DecidedAt { get; set; }
    [Column("decided_by"), Nullable] public Guid? DecidedBy { get; set; }
}

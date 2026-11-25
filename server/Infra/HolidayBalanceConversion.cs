using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_balance_conversions")]
public class HolidayBalanceConversion
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("work_schedule_id")] public Guid WorkScheduleId { get; set; }
    [Column("previous_schedule_id")] public Guid PreviousScheduleId { get; set; }
    [Column("account")] public HolidayAccount Account { get; set; }
    [Column("earning_start")] public DateOnly EarningStart { get; set; }
    [Column("earning_end")] public DateOnly EarningEnd { get; set; }
    [Column("usage_start")] public DateOnly UsageStart { get; set; }
    [Column("usage_end")] public DateOnly UsageEnd { get; set; }
    [Column("date")] public DateOnly Date { get; set; }
    [Column("remaining_days")] public decimal RemainingDays { get; set; }
    [Column("before_hours")] public decimal BeforeHours { get; set; }
    [Column("after_hours")] public decimal AfterHours { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("basis_key")] public string BasisKey { get; set; } = "";
    [Column("created_at")] public DateTime CreatedAt { get; set; }
}

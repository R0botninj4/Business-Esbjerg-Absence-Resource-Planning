using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_usage")]
public class HolidayUsage
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("request_day_id")] public Guid RequestDayId { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("accrual_day_id"), Nullable] public Guid? AccrualDayId { get; set; }
    [Column("date")] public DateOnly Date { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("account")] public HolidayAccount Account { get; set; }
    [Column("earning_start")] public DateOnly EarningStart { get; set; }
    [Column("earning_end")] public DateOnly EarningEnd { get; set; }
    [Column("usage_start")] public DateOnly UsageStart { get; set; }
    [Column("usage_end")] public DateOnly UsageEnd { get; set; }
    [Column("day_length")] public decimal DayLength { get; set; }
    [Column("days")] public decimal Days { get; set; }
}

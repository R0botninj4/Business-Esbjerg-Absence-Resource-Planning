using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_usage")]
public class HolidayUsage
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("request_day_id")] public Guid RequestDayId { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("accrual_day_id")] public Guid AccrualDayId { get; set; }
    [Column("date")] public DateOnly Date { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
}

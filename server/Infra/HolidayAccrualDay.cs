using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_accrual_days")]
public class HolidayAccrualDay
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("accrual_month_id"), NotNull]
    public Guid AccrualMonthId { get; set; }

    [Column("employment_id"), NotNull]
    public Guid EmploymentId { get; set; }

    [Column("date"), NotNull]
    public DateOnly Date { get; set; }

    [Column("account"), NotNull]
    public HolidayAccount Account { get; set; }

    [Column("hours"), NotNull]
    public decimal Hours { get; set; }

    [Column("raw_hours"), NotNull]
    public decimal RawHours { get; set; }

    [Column("raw_days"), NotNull]
    public decimal RawDays { get; set; }

    [Column("holiday_rule_id"), NotNull]
    public Guid HolidayRuleId { get; set; }

    [Column("work_schedule_id"), NotNull]
    public Guid WorkScheduleId { get; set; }

    [Column("earning_start"), NotNull]
    public DateOnly EarningStart { get; set; }

    [Column("earning_end"), NotNull]
    public DateOnly EarningEnd { get; set; }

    [Column("usage_start"), NotNull]
    public DateOnly UsageStart { get; set; }

    [Column("usage_end"), NotNull]
    public DateOnly UsageEnd { get; set; }
}
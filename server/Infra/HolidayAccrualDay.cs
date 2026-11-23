using LinqToDB.Mapping;

namespace Infra;

[Table("HolidayAccrualDays")]
public class HolidayAccrualDay
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public Guid AccrualMonthId { get; set; }

    [Column, NotNull]
    public Guid EmploymentId { get; set; }

    [Column, NotNull]
    public DateOnly Date { get; set; }

    [Column, NotNull]
    public HolidayAccount Account { get; set; }

    [Column, NotNull]
    public decimal Hours { get; set; }

    [Column, NotNull]
    // Bevar det præcise tal, så afrunding kan føres videre til næste måned.
    public decimal RawHours { get; set; }

    [Column, NotNull]
    public decimal RawDays { get; set; }

    [Column, NotNull]
    public Guid HolidayRuleId { get; set; }

    [Column, NotNull]
    public Guid WorkScheduleId { get; set; }

    [Column, NotNull]
    public DateOnly EarningStart { get; set; }

    [Column, NotNull]
    public DateOnly EarningEnd { get; set; }

    [Column, NotNull]
    public DateOnly UsageStart { get; set; }

    [Column, NotNull]
    public DateOnly UsageEnd { get; set; }
}

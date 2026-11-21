using LinqToDB.Mapping;

namespace Infra;

[Table("HolidayRules")]
public class HolidayRule
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public string Name { get; set; } = "";

    [Column, NotNull]
    public DateOnly EffectiveFrom { get; set; }

    [Column, NotNull]
    public int Version { get; set; }

    [Column, NotNull]
    public decimal HolidayDaysPerMonth { get; set; }

    [Column, NotNull]
    public bool SixthWeekEnabled { get; set; }

    [Column, NotNull]
    public decimal SixthWeekHoursPerMonth { get; set; }

    [Column, NotNull]
    public decimal FullTimeHoursPerWeek { get; set; }

    [Column, NotNull]
    public decimal MaximumDebtDays { get; set; }

    [Column, NotNull]
    public bool AllowTransfer { get; set; }

    [Column, NotNull]
    public decimal MaximumTransferDays { get; set; }

    // Perioderne gemmes i deres egen tabel.
    [NotColumn]
    public HolidayRulePeriodSettings? Periods { get; set; }

    [Column, NotNull]
    public DateTime CreatedAt { get; set; }

    [Column, NotNull]
    public Guid CreatedBy { get; set; }
}

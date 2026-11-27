using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_rules")]
public class HolidayRule
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("name"), NotNull]
    public string Name { get; set; } = "";

    [Column("effective_from"), NotNull]
    public DateOnly EffectiveFrom { get; set; }

    [Column("version"), NotNull]
    public int Version { get; set; }

    [Column("holiday_days_per_month"), NotNull]
    public decimal HolidayDaysPerMonth { get; set; }

    [Column("holiday_days_per_year"), Nullable]
    public decimal? HolidayDaysPerYear { get; set; }

    [Column("sixth_week_hours_per_calendar_day"), Nullable]
    public decimal? SixthWeekHoursPerCalendarDay { get; set; }

    [Column("sixth_week_enabled"), NotNull]
    public bool SixthWeekEnabled { get; set; }

    [Column("sixth_week_hours_per_month"), NotNull]
    public decimal SixthWeekHoursPerMonth { get; set; }

    [Column("full_time_hours_per_week"), NotNull]
    public decimal FullTimeHoursPerWeek { get; set; }

    [Column("maximum_debt_days"), NotNull]
    public decimal MaximumDebtDays { get; set; }

    [Column("allow_transfer"), NotNull]
    public bool AllowTransfer { get; set; }

    [Column("maximum_transfer_days"), NotNull]
    public decimal MaximumTransferDays { get; set; }
    [Column("non_transferable_holiday_days")] public decimal NonTransferableHolidayDays { get; set; } = 20;
    [Column("sickness_waiting_days")] public int SicknessWaitingDays { get; set; } = 5;
    
    [NotColumn]
    public HolidayRulePeriodSettings? Periods { get; set; }

    [Column("created_at"), NotNull]
    public DateTime CreatedAt { get; set; }

    [Column("created_by"), NotNull]
    public Guid CreatedBy { get; set; }
}

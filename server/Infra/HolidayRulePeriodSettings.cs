using LinqToDB.Mapping;

namespace Infra;

[Table("HolidayRulePeriods")]
public class HolidayRulePeriodSettings
{
    [PrimaryKey, Column]
    public Guid HolidayRuleId { get; set; }

    // Måned og dag gentages hvert år.
    // YearOffset er antal år efter optjeningsårets startår.
    [Column, NotNull]
    public int HolidayEarningStartMonth { get; set; }

    [Column, NotNull]
    public int HolidayEarningStartDay { get; set; }

    [Column, NotNull]
    public int HolidayUsageStartMonth { get; set; }

    [Column, NotNull]
    public int HolidayUsageStartDay { get; set; }

    [Column, NotNull]
    public int HolidayUsageStartYearOffset { get; set; }

    [Column, NotNull]
    public int HolidayUsageEndMonth { get; set; }

    [Column, NotNull]
    public int HolidayUsageEndDay { get; set; }

    [Column, NotNull]
    public int HolidayUsageEndYearOffset { get; set; }

    [Column, NotNull]
    public int SixthWeekEarningStartMonth { get; set; }

    [Column, NotNull]
    public int SixthWeekEarningStartDay { get; set; }

    [Column, NotNull]
    public int SixthWeekUsageStartMonth { get; set; }

    [Column, NotNull]
    public int SixthWeekUsageStartDay { get; set; }

    [Column, NotNull]
    public int SixthWeekUsageStartYearOffset { get; set; }

    [Column, NotNull]
    public int SixthWeekUsageEndMonth { get; set; }

    [Column, NotNull]
    public int SixthWeekUsageEndDay { get; set; }

    [Column, NotNull]
    public int SixthWeekUsageEndYearOffset { get; set; }

    [Column, NotNull]
    public int MainHolidayDays { get; set; }

    [Column, NotNull]
    public int MainHolidayStartMonth { get; set; }

    [Column, NotNull]
    public int MainHolidayStartDay { get; set; }

    [Column, NotNull]
    public int MainHolidayEndMonth { get; set; }

    [Column, NotNull]
    public int MainHolidayEndDay { get; set; }

    [Column, NotNull]
    public int MainHolidayNoticeMonths { get; set; }

    [Column, NotNull]
    public int OtherHolidayNoticeMonths { get; set; }

    [Column, NotNull]
    public int DebtDeadlineMonth { get; set; }

    [Column, NotNull]
    public int DebtDeadlineDay { get; set; }

    [Column, NotNull]
    public int HolidayTransferDeadlineMonth { get; set; }

    [Column, NotNull]
    public int HolidayTransferDeadlineDay { get; set; }

    [Column, NotNull]
    public int SixthWeekTransferDeadlineMonth { get; set; }

    [Column, NotNull]
    public int SixthWeekTransferDeadlineDay { get; set; }
}

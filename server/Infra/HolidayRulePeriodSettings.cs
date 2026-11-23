using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_rule_periods")]
public class HolidayRulePeriodSettings
{
    [PrimaryKey, Column("holiday_rule_id")]
    public Guid HolidayRuleId { get; set; }
    
    [Column("holiday_earning_start_month"), NotNull]
    public int HolidayEarningStartMonth { get; set; }

    [Column("holiday_earning_start_day"), NotNull]
    public int HolidayEarningStartDay { get; set; }

    [Column("holiday_usage_start_month"), NotNull]
    public int HolidayUsageStartMonth { get; set; }

    [Column("holiday_usage_start_day"), NotNull]
    public int HolidayUsageStartDay { get; set; }

    [Column("holiday_usage_start_year_offset"), NotNull]
    public int HolidayUsageStartYearOffset { get; set; }

    [Column("holiday_usage_end_month"), NotNull]
    public int HolidayUsageEndMonth { get; set; }

    [Column("holiday_usage_end_day"), NotNull]
    public int HolidayUsageEndDay { get; set; }

    [Column("holiday_usage_end_year_offset"), NotNull]
    public int HolidayUsageEndYearOffset { get; set; }

    [Column("sixth_week_earning_start_month"), NotNull]
    public int SixthWeekEarningStartMonth { get; set; }

    [Column("sixth_week_earning_start_day"), NotNull]
    public int SixthWeekEarningStartDay { get; set; }

    [Column("sixth_week_usage_start_month"), NotNull]
    public int SixthWeekUsageStartMonth { get; set; }

    [Column("sixth_week_usage_start_day"), NotNull]
    public int SixthWeekUsageStartDay { get; set; }

    [Column("sixth_week_usage_start_year_offset"), NotNull]
    public int SixthWeekUsageStartYearOffset { get; set; }

    [Column("sixth_week_usage_end_month"), NotNull]
    public int SixthWeekUsageEndMonth { get; set; }

    [Column("sixth_week_usage_end_day"), NotNull]
    public int SixthWeekUsageEndDay { get; set; }

    [Column("sixth_week_usage_end_year_offset"), NotNull]
    public int SixthWeekUsageEndYearOffset { get; set; }

    [Column("main_holiday_days"), NotNull]
    public int MainHolidayDays { get; set; }

    [Column("main_holiday_start_month"), NotNull]
    public int MainHolidayStartMonth { get; set; }

    [Column("main_holiday_start_day"), NotNull]
    public int MainHolidayStartDay { get; set; }

    [Column("main_holiday_end_month"), NotNull]
    public int MainHolidayEndMonth { get; set; }

    [Column("main_holiday_end_day"), NotNull]
    public int MainHolidayEndDay { get; set; }

    [Column("main_holiday_notice_months"), NotNull]
    public int MainHolidayNoticeMonths { get; set; }

    [Column("other_holiday_notice_months"), NotNull]
    public int OtherHolidayNoticeMonths { get; set; }

    [Column("debt_deadline_month"), NotNull]
    public int DebtDeadlineMonth { get; set; }

    [Column("debt_deadline_day"), NotNull]
    public int DebtDeadlineDay { get; set; }

    [Column("holiday_transfer_deadline_month"), NotNull]
    public int HolidayTransferDeadlineMonth { get; set; }

    [Column("holiday_transfer_deadline_day"), NotNull]
    public int HolidayTransferDeadlineDay { get; set; }

    [Column("sixth_week_transfer_deadline_month"), NotNull]
    public int SixthWeekTransferDeadlineMonth { get; set; }

    [Column("sixth_week_transfer_deadline_day"), NotNull]
    public int SixthWeekTransferDeadlineDay { get; set; }
}
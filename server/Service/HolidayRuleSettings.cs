using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Service;

public class HolidayRuleSettings
{
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    // Felterne skal med i JSON, så en glemt sats ikke bliver gemt som 0.
    [JsonRequired]
    public decimal HolidayDaysPerMonth { get; set; }

    [JsonRequired]
    public bool SixthWeekEnabled { get; set; }

    [JsonRequired]
    public decimal SixthWeekHoursPerMonth { get; set; }

    [JsonRequired]
    public decimal FullTimeHoursPerWeek { get; set; }

    [JsonRequired]
    public decimal MaximumDebtDays { get; set; }

    [JsonRequired]
    public bool AllowTransfer { get; set; }

    [JsonRequired]
    public decimal MaximumTransferDays { get; set; }

    // Uge 1–5: optjeningsårets start og afholdelsesperioden.
    [JsonRequired]
    public int HolidayEarningStartMonth { get; set; }

    [JsonRequired]
    public int HolidayEarningStartDay { get; set; }

    [JsonRequired]
    public int HolidayUsageStartMonth { get; set; }

    [JsonRequired]
    public int HolidayUsageStartDay { get; set; }

    // 0 er optjeningsårets startår. 1 er året efter.
    [JsonRequired]
    public int HolidayUsageStartYearOffset { get; set; }

    [JsonRequired]
    public int HolidayUsageEndMonth { get; set; }

    [JsonRequired]
    public int HolidayUsageEndDay { get; set; }

    [JsonRequired]
    public int HolidayUsageEndYearOffset { get; set; }

    // 6. ferieuge har sit eget optjeningsår og sin egen afholdelsesperiode.
    [JsonRequired]
    public int SixthWeekEarningStartMonth { get; set; }

    [JsonRequired]
    public int SixthWeekEarningStartDay { get; set; }

    [JsonRequired]
    public int SixthWeekUsageStartMonth { get; set; }

    [JsonRequired]
    public int SixthWeekUsageStartDay { get; set; }

    [JsonRequired]
    public int SixthWeekUsageStartYearOffset { get; set; }

    [JsonRequired]
    public int SixthWeekUsageEndMonth { get; set; }

    [JsonRequired]
    public int SixthWeekUsageEndDay { get; set; }

    [JsonRequired]
    public int SixthWeekUsageEndYearOffset { get; set; }

    // Hovedferie og varsler.
    [JsonRequired]
    public int MainHolidayDays { get; set; }

    [JsonRequired]
    public int MainHolidayStartMonth { get; set; }

    [JsonRequired]
    public int MainHolidayStartDay { get; set; }

    [JsonRequired]
    public int MainHolidayEndMonth { get; set; }

    [JsonRequired]
    public int MainHolidayEndDay { get; set; }

    [JsonRequired]
    public int MainHolidayNoticeMonths { get; set; }

    [JsonRequired]
    public int OtherHolidayNoticeMonths { get; set; }

    // Datoerne gentages årligt. Overførselsfristen følger afholdelsesperioden.
    [JsonRequired]
    public int DebtDeadlineMonth { get; set; }

    [JsonRequired]
    public int DebtDeadlineDay { get; set; }

    [JsonRequired]
    public int HolidayTransferDeadlineMonth { get; set; }

    [JsonRequired]
    public int HolidayTransferDeadlineDay { get; set; }

    [JsonRequired]
    public int SixthWeekTransferDeadlineMonth { get; set; }

    [JsonRequired]
    public int SixthWeekTransferDeadlineDay { get; set; }
}

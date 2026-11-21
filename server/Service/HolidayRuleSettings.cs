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
}

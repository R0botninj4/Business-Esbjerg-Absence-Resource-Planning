using Infra;

namespace Service;

public class HolidayOpeningBalanceRequest
{
    public Guid OperationId { get; set; }
    public HolidayAccount Account { get; set; }
    public decimal Hours { get; set; }
    public DateOnly EarningStart { get; set; }
    public DateOnly EarningEnd { get; set; }
    public DateOnly UsageStart { get; set; }
    public DateOnly UsageEnd { get; set; }
    public string Reason { get; set; } = "";
}

namespace Service;

public class HolidayTransferInput
{
    public Guid OperationId { get; set; }
    // En bogført optjeningspost bestemmer medarbejder, konto og kildeperiode.
    public Guid AccrualDayId { get; set; }
    public decimal Hours { get; set; }
}

public class HolidayTransferDecisionInput
{
    public bool Approve { get; set; }
    public string AgreementReference { get; set; } = "";
    public string Comment { get; set; } = "";
}

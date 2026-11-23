namespace Service;

public class HolidayAdjustmentRequest
{
    // Behold samme ID, hvis det samme kald forsøges igen.
    public Guid OperationId { get; set; }

    // ID fra en bogført optjeningspost. Den bestemmer konto og periode.
    public Guid AccrualDayId { get; set; }

    public DateOnly Date { get; set; }

    // Positivt tal tilføjer timer. Negativt tal trækker timer fra.
    public decimal Hours { get; set; }

    public string Reason { get; set; } = "";
}

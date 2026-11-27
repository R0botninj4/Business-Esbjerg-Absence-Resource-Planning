namespace Service;

public class HolidayObstacleInput
{
    public Guid OperationId { get; set; }
    public Guid? RequestId { get; set; }
    // sicknessBefore, sicknessDuring, parentalLeave, strike eller other.
    public string Kind { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    // Reference til dokumentation. Selve dokumentet gemmes ikke i kalenderen.
    public string DocumentationReference { get; set; } = "";
}

public class HolidayObstacleResult
{
    public Infra.HolidayObstacle Report { get; set; } = new();
    public List<Infra.HolidayObstacleDay> Days { get; set; } = new();
}

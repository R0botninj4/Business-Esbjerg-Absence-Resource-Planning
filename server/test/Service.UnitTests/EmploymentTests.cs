using Service;

namespace Service.UnitTests;

public class EmploymentTests
{
    [Fact]
    public void ValidateDates_RejectsEndDateBeforeStartDate()
    {
        // Slutdatoen ligger en dag før startdatoen.
        var startDate = new DateOnly(2026, 11, 17);
        var endDate = new DateOnly(2026, 11, 16);

        // Assert.Throws tjekker, at metoden afviser datoerne.
        Assert.Throws<ArgumentException>("endDate", () =>
            EmploymentService.ValidateDates(startDate, endDate));
    }
}

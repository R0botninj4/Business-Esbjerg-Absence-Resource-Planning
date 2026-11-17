using Service;

namespace Service.UnitTests;

public class EmploymentTests
{
    [Fact]
    public void ValidateDates_RejectsEndDateBeforeStartDate()
    {
        var startDate = new DateOnly(2026, 11, 17);
        var endDate = new DateOnly(2026, 11, 16);

        Assert.Throws<ArgumentException>("endDate", () =>
            EmploymentService.ValidateDates(startDate, endDate));
    }
}

using Service;

namespace Service.UnitTests;

public class WorkScheduleTests
{
    [Fact]
    public void ValidateDays_AcceptsWorkingDaysAndDayOff()
    {
        var days = CreateDays();
        days[0].IsDayOff = false;
        days[0].StartTime = new TimeOnly(8, 0);
        days[0].EndTime = new TimeOnly(14, 0);

        WorkScheduleService.ValidateDays(days);
    }

    [Fact]
    public void ValidateDays_RejectsDuplicateWeekday()
    {
        var days = CreateDays();
        days[4].Day = DayOfWeek.Monday;

        Assert.Throws<ArgumentException>(() => WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidateDays_RejectsTimesOnDayOff()
    {
        // Lav en ugeplan og ret mandagens oplysninger.
        var days = CreateDays();
        days[0].IsDayOff = true;
        days[0].StartTime = new TimeOnly(8, 0);
        days[0].EndTime = new TimeOnly(14, 0);

        // Planen skal blive afvist.
        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidateDays_RejectsMissingStartTime()
    {
        // Lav en ugeplan og ret mandagens oplysninger.
        var days = CreateDays();
        days[0].IsDayOff = false;
        days[0].StartTime = null;
        days[0].EndTime = new TimeOnly(14, 0);

        // Planen skal blive afvist.
        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidateDays_RejectsMissingEndTime()
    {
        // Lav en ugeplan og ret mandagens oplysninger.
        var days = CreateDays();
        days[0].IsDayOff = false;
        days[0].StartTime = new TimeOnly(8, 0);
        days[0].EndTime = null;

        // Planen skal blive afvist.
        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidateDays_RejectsEndBeforeStart()
    {
        // Lav en ugeplan og ret mandagens oplysninger.
        var days = CreateDays();
        days[0].IsDayOff = false;
        days[0].StartTime = new TimeOnly(14, 0);
        days[0].EndTime = new TimeOnly(8, 0);

        // Planen skal blive afvist.
        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidateDays_RejectsEqualTimes()
    {
        // Lav en ugeplan og ret mandagens oplysninger.
        var days = CreateDays();
        days[0].IsDayOff = false;
        days[0].StartTime = new TimeOnly(8, 0);
        days[0].EndTime = new TimeOnly(8, 0);

        // Planen skal blive afvist.
        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidatePeriod_RejectsStartBeforeEmployment()
    {
        var employment = new Infra.Employment
        {
            StartDate = new DateOnly(2026, 12, 10),
            EndDate = new DateOnly(2026, 12, 20)
        };
        var startDate = new DateOnly(2026, 12, 1);
        DateOnly? endDate = new DateOnly(2026, 12, 31);

        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidatePeriod(employment, startDate, endDate));
    }

    [Fact]
    public void ValidatePeriod_RejectsEndBeforeStart()
    {
        var employment = new Infra.Employment
        {
            StartDate = new DateOnly(2026, 12, 10),
            EndDate = new DateOnly(2026, 12, 20)
        };
        var startDate = new DateOnly(2026, 12, 10);
        DateOnly? endDate = new DateOnly(2026, 12, 9);

        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidatePeriod(employment, startDate, endDate));
    }

    [Fact]
    public void ValidatePeriod_RejectsEndAfterEmployment()
    {
        var employment = new Infra.Employment
        {
            StartDate = new DateOnly(2026, 12, 10),
            EndDate = new DateOnly(2026, 12, 20)
        };
        var startDate = new DateOnly(2026, 12, 10);
        DateOnly? endDate = new DateOnly(2026, 12, 31);

        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidatePeriod(employment, startDate, endDate));
    }

    [Fact]
    public void ValidatePeriod_RejectsOpenPlanForEndedEmployment()
    {
        var employment = new Infra.Employment
        {
            StartDate = new DateOnly(2026, 12, 10),
            EndDate = new DateOnly(2026, 12, 20)
        };
        var startDate = new DateOnly(2026, 12, 10);
        DateOnly? endDate = null;

        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidatePeriod(employment, startDate, endDate));
    }

    [Fact]
    public void ValidateDays_RejectsMissingDay()
    {
        var days = new WorkScheduleDayRequest[5];

        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    [Fact]
    public void ValidateDays_RejectsEmptyWeek()
    {
        var days = new WorkScheduleDayRequest[0];

        Assert.Throws<ArgumentException>(() =>
            WorkScheduleService.ValidateDays(days));
    }

    private static WorkScheduleDayRequest[] CreateDays()
    {
        var days = new WorkScheduleDayRequest[5];
        for (int i = 0; i < 5; i++)
        {
            days[i] = new WorkScheduleDayRequest
            {
                Day = (DayOfWeek)(i + 1),
                IsDayOff = true
            };
        }
        return days;
    }
}

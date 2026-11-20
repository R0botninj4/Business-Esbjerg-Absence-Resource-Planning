using System.Net;
using System.Net.Http.Json;
using Infra;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace Api.IntegrationTests;

public class WorkScheduleTests
{
    private const string Password = "ScheduleTestPassword123!";

    [Fact]
    public async Task PartTime_RequiresValidDays_AndCreatesPlan()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var client = LoginTests.CreateClient(factory);
            Guid employeeId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create("admin@businessesbjerg.com", Password, "Admin");
                employeeId = users.Create("parttime@businessesbjerg.com", Password, "Employee").Id;
            }

            using var login = await client.PostAsJsonAsync(
                "/api/auth/login", new { email = "admin@businessesbjerg.com", password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/users/{employeeId}/employments";

            using var invalid = await client.PostAsJsonAsync(
                path, new { type = 2, startDate = "2026-12-01" });
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            var empty = await client.GetFromJsonAsync<Employment[]>(path);
            Assert.NotNull(empty);
            Assert.Empty(empty);

            var days = CreateDays();
            using var created = await client.PostAsJsonAsync(
                path, new { type = 2, startDate = "2026-12-01", days });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var employment = await created.Content.ReadFromJsonAsync<Employment>();
            Assert.NotNull(employment);
            var plans = await client.GetFromJsonAsync<WorkSchedule[]>(
                $"/api/employments/{employment.Id}/work-schedules");
            Assert.NotNull(plans);
            var plan = Assert.Single(plans);
            var savedDays = await client.GetFromJsonAsync<WorkScheduleDay[]>(
                $"/api/employments/{employment.Id}/work-schedules/{plan.Id}/days");
            Assert.NotNull(savedDays);
            Assert.Equal(5, savedDays.Length);
            Assert.True(savedDays[4].IsDayOff);
        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task Employee_CanReadOwnPlan()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var client = LoginTests.CreateClient(factory);
            Guid ownerId;
            Guid employmentId;
            Guid planId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                var owner = users.Create("owner@businessesbjerg.com", Password, "Employee");
                users.Create("reader@businessesbjerg.com", Password, "Employee");
                ownerId = owner.Id;
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var employment = employments.Create(owner.Id, EmploymentType.FullTime, new DateOnly(2026, 12, 1), null);
                Assert.NotNull(employment);
                employmentId = employment.Id;
                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plans = schedules.GetByEmploymentId(employmentId);
                planId = plans[0].Id;
            }

            var email = "owner@businessesbjerg.com";
            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/employments/{employmentId}/work-schedules";
            using var list = await client.GetAsync(path);
            using var date = await client.GetAsync($"{path}/for-date?date=2026-12-01");
            using var days = await client.GetAsync($"{path}/{planId}/days");
            using var employmentResponse = await client.GetAsync($"/api/users/{ownerId}/employments");
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
            Assert.Equal(HttpStatusCode.OK, date.StatusCode);
            Assert.Equal(HttpStatusCode.OK, days.StatusCode);
            Assert.Equal(HttpStatusCode.OK, employmentResponse.StatusCode);


            using var edit = await client.PutAsJsonAsync(
                $"{path}/{planId}", new { startDate = "2027-01-01", days = CreateDays() });
            Assert.Equal(HttpStatusCode.Forbidden, edit.StatusCode);
            using var create = await client.PostAsJsonAsync(
                path, new { startDate = "2027-01-01", days = CreateDays() });
            Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
            using var end = await client.PatchAsJsonAsync(
                $"{path}/{planId}/end", new { endDate = "2026-12-31" });
            Assert.Equal(HttpStatusCode.Forbidden, end.StatusCode);

        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task Employee_CannotReadAnotherEmployeesPlan()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var client = LoginTests.CreateClient(factory);
            Guid ownerId;
            Guid employmentId;
            Guid planId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                var owner = users.Create("owner@businessesbjerg.com", Password, "Employee");
                users.Create("reader@businessesbjerg.com", Password, "Employee");
                ownerId = owner.Id;
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var employment = employments.Create(owner.Id, EmploymentType.FullTime, new DateOnly(2026, 12, 1), null);
                Assert.NotNull(employment);
                employmentId = employment.Id;
                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plans = schedules.GetByEmploymentId(employmentId);
                planId = plans[0].Id;
            }

            var email = "reader@businessesbjerg.com";
            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/employments/{employmentId}/work-schedules";
            using var list = await client.GetAsync(path);
            using var date = await client.GetAsync($"{path}/for-date?date=2026-12-01");
            using var days = await client.GetAsync($"{path}/{planId}/days");
            using var employmentResponse = await client.GetAsync($"/api/users/{ownerId}/employments");
            Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, date.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, days.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, employmentResponse.StatusCode);


            using var edit = await client.PutAsJsonAsync(
                $"{path}/{planId}", new { startDate = "2027-01-01", days = CreateDays() });
            Assert.Equal(HttpStatusCode.Forbidden, edit.StatusCode);
            using var create = await client.PostAsJsonAsync(
                path, new { startDate = "2027-01-01", days = CreateDays() });
            Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
            using var end = await client.PatchAsJsonAsync(
                $"{path}/{planId}/end", new { endDate = "2026-12-31" });
            Assert.Equal(HttpStatusCode.Forbidden, end.StatusCode);

        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task Accounting_CanReadEmployeesPlan()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var client = LoginTests.CreateClient(factory);
            Guid ownerId;
            Guid employmentId;
            Guid planId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                var owner = users.Create("owner@businessesbjerg.com", Password, "Employee");
                users.Create("reader@businessesbjerg.com", Password, "Accounting");
                ownerId = owner.Id;
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var employment = employments.Create(owner.Id, EmploymentType.FullTime, new DateOnly(2026, 12, 1), null);
                Assert.NotNull(employment);
                employmentId = employment.Id;
                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plans = schedules.GetByEmploymentId(employmentId);
                planId = plans[0].Id;
            }

            var email = "reader@businessesbjerg.com";
            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/employments/{employmentId}/work-schedules";
            using var list = await client.GetAsync(path);
            using var date = await client.GetAsync($"{path}/for-date?date=2026-12-01");
            using var days = await client.GetAsync($"{path}/{planId}/days");
            using var employmentResponse = await client.GetAsync($"/api/users/{ownerId}/employments");
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
            Assert.Equal(HttpStatusCode.OK, date.StatusCode);
            Assert.Equal(HttpStatusCode.OK, days.StatusCode);
            Assert.Equal(HttpStatusCode.OK, employmentResponse.StatusCode);


        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task Admin_CanChangePlanAndKeepHistory()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var client = LoginTests.CreateClient(factory);
            Guid employeeId;
            Guid employmentId;
            Guid oldPlanId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create("editor@businessesbjerg.com", Password, "Admin");
                var employee = users.Create("employee@businessesbjerg.com", Password, "Employee");
                employeeId = employee.Id;
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var employment = employments.Create(employeeId, EmploymentType.PartTime, new DateOnly(2026, 12, 1), null, CreateDays());
                Assert.NotNull(employment);
                employmentId = employment.Id;
                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plans = schedules.GetByEmploymentId(employmentId);
                oldPlanId = plans[0].Id;
            }

            using var login = await client.PostAsJsonAsync("/api/auth/login",
                new { email = "editor@businessesbjerg.com", password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/employments/{employmentId}/work-schedules";
            var change = new { startDate = "2027-01-01", days = CreateDays() };

            using var overlapping = await client.PostAsJsonAsync(path, change);
            Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
            using var invalid = await client.PutAsJsonAsync(
                $"{path}/{oldPlanId}", new { startDate = "2027-01-01", days = new WorkScheduleDayRequest[0] });
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            var original = await client.GetFromJsonAsync<WorkSchedule[]>(path);
            Assert.NotNull(original);
            Assert.Single(original);
            Assert.Null(original[0].EndDate);

            using var edited = await client.PutAsJsonAsync($"{path}/{oldPlanId}", change);
            Assert.Equal(HttpStatusCode.Created, edited.StatusCode);
            var newPlan = await edited.Content.ReadFromJsonAsync<WorkSchedule>();
            Assert.NotNull(newPlan);
            var december = await client.GetFromJsonAsync<WorkSchedule>(
                $"{path}/for-date?date=2026-12-31");
            Assert.NotNull(december);
            var january = await client.GetFromJsonAsync<WorkSchedule>(
                $"{path}/for-date?date=2027-01-01");
            Assert.NotNull(january);
            Assert.Equal(oldPlanId, december.Id);
            Assert.Equal(new DateOnly(2026, 12, 31), december.EndDate);
            Assert.Equal(newPlan.Id, january.Id);
            var oldDays = await client.GetFromJsonAsync<WorkScheduleDay[]>($"{path}/{oldPlanId}/days");
            Assert.NotNull(oldDays);
            Assert.Equal(5, oldDays.Length);

            using var repeated = await client.PutAsJsonAsync($"{path}/{oldPlanId}", change);
            Assert.Equal(HttpStatusCode.BadRequest, repeated.StatusCode);
            using (var scope = factory.Services.CreateScope())
            {
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var ended = employments.End(employeeId, employmentId, new DateOnly(2027, 1, 31));
                Assert.True(ended);
            }
            using var afterEmployment = await client.GetAsync($"{path}/for-date?date=2027-02-01");
            Assert.Equal(HttpStatusCode.NotFound, afterEmployment.StatusCode);
        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task Accounting_CanChangePlanAndKeepHistory()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var client = LoginTests.CreateClient(factory);
            Guid employeeId;
            Guid employmentId;
            Guid oldPlanId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create("editor@businessesbjerg.com", Password, "Accounting");
                var employee = users.Create("employee@businessesbjerg.com", Password, "Employee");
                employeeId = employee.Id;
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var employment = employments.Create(employeeId, EmploymentType.PartTime, new DateOnly(2026, 12, 1), null, CreateDays());
                Assert.NotNull(employment);
                employmentId = employment.Id;
                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plans = schedules.GetByEmploymentId(employmentId);
                oldPlanId = plans[0].Id;
            }

            using var login = await client.PostAsJsonAsync("/api/auth/login",
                new { email = "editor@businessesbjerg.com", password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/employments/{employmentId}/work-schedules";
            var change = new { startDate = "2027-01-01", days = CreateDays() };

            using var overlapping = await client.PostAsJsonAsync(path, change);
            Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
            using var invalid = await client.PutAsJsonAsync(
                $"{path}/{oldPlanId}", new { startDate = "2027-01-01", days = new WorkScheduleDayRequest[0] });
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            var original = await client.GetFromJsonAsync<WorkSchedule[]>(path);
            Assert.NotNull(original);
            Assert.Single(original);
            Assert.Null(original[0].EndDate);

            using var edited = await client.PutAsJsonAsync($"{path}/{oldPlanId}", change);
            Assert.Equal(HttpStatusCode.Created, edited.StatusCode);
            var newPlan = await edited.Content.ReadFromJsonAsync<WorkSchedule>();
            Assert.NotNull(newPlan);
            var december = await client.GetFromJsonAsync<WorkSchedule>(
                $"{path}/for-date?date=2026-12-31");
            Assert.NotNull(december);
            var january = await client.GetFromJsonAsync<WorkSchedule>(
                $"{path}/for-date?date=2027-01-01");
            Assert.NotNull(january);
            Assert.Equal(oldPlanId, december.Id);
            Assert.Equal(new DateOnly(2026, 12, 31), december.EndDate);
            Assert.Equal(newPlan.Id, january.Id);
            var oldDays = await client.GetFromJsonAsync<WorkScheduleDay[]>($"{path}/{oldPlanId}/days");
            Assert.NotNull(oldDays);
            Assert.Equal(5, oldDays.Length);

            using var repeated = await client.PutAsJsonAsync($"{path}/{oldPlanId}", change);
            Assert.Equal(HttpStatusCode.BadRequest, repeated.StatusCode);
            using (var scope = factory.Services.CreateScope())
            {
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var ended = employments.End(employeeId, employmentId, new DateOnly(2027, 1, 31));
                Assert.True(ended);
            }
            using var afterEmployment = await client.GetAsync($"{path}/for-date?date=2027-02-01");
            Assert.Equal(HttpStatusCode.NotFound, afterEmployment.StatusCode);
        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public void FullTimeToPartTime_KeepsBothWorkSchedules()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var scope = factory.Services.CreateScope();
            var employee = scope.ServiceProvider.GetRequiredService<UserService>()
                .Create("changing@businessesbjerg.com", Password, "Employee");
            var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
            var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();

            var fullTime = employments.Create(
                employee.Id, EmploymentType.FullTime, new DateOnly(2026, 12, 1), null);
            Assert.NotNull(fullTime);
            Assert.True(employments.End(employee.Id, fullTime.Id, new DateOnly(2026, 12, 31)));

            var partTime = employments.Create(
                employee.Id, EmploymentType.PartTime, new DateOnly(2027, 1, 1), null, CreateDays());
            Assert.NotNull(partTime);
            var oldPlan = Assert.Single(schedules.GetByEmploymentId(fullTime.Id));
            var newPlan = Assert.Single(schedules.GetByEmploymentId(partTime.Id));
            Assert.NotNull(newPlan);
            Assert.Equal(new DateOnly(2026, 12, 31), oldPlan.EndDate);
            Assert.Equal(new DateOnly(2027, 1, 1), newPlan.StartDate);
            Assert.Null(schedules.GetForDate(fullTime.Id, new DateOnly(2027, 1, 1)));
            Assert.Equal(new TimeOnly(15, 30), schedules.GetDays(oldPlan.Id)[0].EndTime);
            Assert.Equal(new TimeOnly(14, 0), schedules.GetDays(newPlan.Id)[0].EndTime);
        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    [Fact]
    public void InvalidFullTimeConfiguration_RollsBackEmployment()
    {
        // Hver test får sin egen database, så testene ikke påvirker hinanden.
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
            using var scope = factory.Services.CreateScope();
            var employee = scope.ServiceProvider.GetRequiredService<UserService>()
                .Create("rollback@businessesbjerg.com", Password, "Employee");
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            configuration["FullTimeWorkSchedule:0:EndTime"] = "07:00:00";
            var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
            Assert.Throws<ArgumentException>(() =>
                employments.Create(employee.Id, EmploymentType.FullTime, new DateOnly(2026, 12, 1), null));
            Assert.Empty(employments.GetByUserId(employee.Id));
            var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
            Assert.Equal(0, db.WorkSchedules.Count());
            Assert.Equal(0, db.WorkScheduleDays.Count());
        }
        finally
        {
            // Ryd testens database op bagefter.
            File.Delete(databasePath);
        }
    }

    private static WorkScheduleDayRequest[] CreateDays()
    {
        var days = new WorkScheduleDayRequest[5];
        for (int i = 0; i < 5; i++)
        {
            days[i] = new WorkScheduleDayRequest
            {
                Day = (DayOfWeek)(i + 1),
                IsDayOff = i == 4
            };
            if (!days[i].IsDayOff)
            {
                days[i].StartTime = new TimeOnly(8, 0);
                days[i].EndTime = new TimeOnly(14, 0);
            }
        }
        return days;
    }
}

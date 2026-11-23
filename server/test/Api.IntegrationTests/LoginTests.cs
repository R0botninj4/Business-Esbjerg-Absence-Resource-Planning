using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Service;

namespace Api.IntegrationTests;

public class LoginTests
{
    [Fact]
    public async Task Login_WithValidCredentials_GivesAccessToProfile()
    {
        const string email = "employee@businessesbjerg.com";
        const string password = "LocalTestPassword123!";
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = CreateFactory(schema);
            using var client = CreateClient(factory);

            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create(email, password, "Employee");
            }

            using var beforeLogin = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, beforeLogin.StatusCode);

            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);

            using var profile = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.OK, profile.StatusCode);

            var body = await profile.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(email, body.GetProperty("email").GetString());
            Assert.Equal("Employee", body.GetProperty("role").GetString());
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        const string email = "wrong-password@businessesbjerg.com";
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = CreateFactory(schema);
            using var client = CreateClient(factory);

            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create(email, "LocalTestPassword123!", "Employee");
            }

            using var login = await client.PostAsJsonAsync(
                "/api/auth/login", new { email, password = "WrongPassword123!" });
            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);

            using var profile = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, profile.StatusCode);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public async Task Logout_EndsTheSession()
    {
        const string email = "logout@businessesbjerg.com";
        const string password = "LocalTestPassword123!";
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = CreateFactory(schema);
            using var client = CreateClient(factory);

            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create(email, password, "Accounting");
            }

            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);

            using var logout = await client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

            using var profile = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, profile.StatusCode);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public async Task Admin_CanManageUsers_AndDeletedUserLosesAccess()
    {
        const string adminEmail = "admin-crud@businessesbjerg.com";
        const string email = "employee-crud@businessesbjerg.com";
        const string password = "CrudTestPassword123!";
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = CreateFactory(schema);
            using var admin = CreateClient(factory);
            using var employee = CreateClient(factory);

            var openApi = await admin.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
            var paths = openApi.GetProperty("paths");
            Assert.True(paths.GetProperty("/api/users").TryGetProperty("post", out _));
            Assert.True(paths.GetProperty("/api/users/{id}").TryGetProperty("put", out _));
            Assert.True(paths.GetProperty("/api/users/{id}").TryGetProperty("delete", out _));

            using var anonymous = await admin.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create(adminEmail, password, "Admin");
            }

            using var adminLogin = await admin.PostAsJsonAsync(
                "/api/auth/login", new { email = adminEmail, password });
            Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);

            var request = new { email, password, role = "Employee" };
            using var created = await admin.PostAsJsonAsync("/api/users", request);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var body = await created.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(body.TryGetProperty("passwordHash", out _));
            Assert.NotNull(created.Headers.Location);
            var userPath = created.Headers.Location.PathAndQuery;

            using var duplicate = await admin.PostAsJsonAsync("/api/users", request);
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

            using var listed = await admin.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
            var list = await listed.Content.ReadFromJsonAsync<JsonElement>();
            bool foundEmployee = false;
            foreach (var user in list.EnumerateArray())
            {
                if (user.GetProperty("email").GetString() == email)
                {
                    foundEmployee = true;
                }
            }
            Assert.True(foundEmployee);

            using var updated = await admin.PutAsJsonAsync(userPath, new { email, role = "Employee" });
            Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
            using var found = await admin.GetAsync(userPath);
            Assert.Equal(HttpStatusCode.OK, found.StatusCode);
            var profile = await found.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Employee", profile.GetProperty("role").GetString());

            using var employeeLogin = await employee.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.OK, employeeLogin.StatusCode);
            using var forbidden = await employee.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

            using var deactivated = await admin.DeleteAsync(userPath);
            Assert.Equal(HttpStatusCode.NoContent, deactivated.StatusCode);

            using var retainedUser = await admin.GetAsync(userPath);
            Assert.Equal(HttpStatusCode.OK, retainedUser.StatusCode);
            var retainedProfile = await retainedUser.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(retainedProfile.GetProperty("isActive").GetBoolean());

            using var expiredSession = await employee.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, expiredSession.StatusCode);

            using var retryLogin = await employee.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.Unauthorized, retryLogin.StatusCode);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public async Task Admin_CannotCreateOverlappingEmployment()
    {
        const string adminEmail = "employment-admin@businessesbjerg.com";
        const string password = "EmploymentTestPassword123!";
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = CreateFactory(schema);
            using var client = CreateClient(factory);

            Guid employeeId;
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create(adminEmail, password, "Admin");
                var employee = users.Create("employment-employee@businessesbjerg.com", password, "Employee");
                employeeId = employee.Id;
            }

            using var login = await client.PostAsJsonAsync(
                "/api/auth/login", new { email = adminEmail, password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);

            var path = $"/api/users/{employeeId}/employments";
            var period = new { type = EmploymentType.FullTime, startDate = new DateOnly(2026, 11, 17) };

            using var first = await client.PostAsJsonAsync(path, period);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var employment = await first.Content.ReadFromJsonAsync<Employment>();
            Assert.NotNull(employment);
            using (var scope = factory.Services.CreateScope())
            {
                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plans = schedules.GetByEmploymentId(employment.Id);
                var plan = Assert.Single(plans);
                Assert.Equal(employment.StartDate, plan.StartDate);
                Assert.Equal(employment.EndDate, plan.EndDate);

                var days = schedules.GetDays(plan.Id);
                Assert.Equal(5, days.Length);
                double totalHours = 0;
                foreach (var day in days)
                {
                    Assert.False(day.IsDayOff);
                    Assert.True(day.StartTime.HasValue);
                    Assert.True(day.EndTime.HasValue);
                    var startTime = day.StartTime.GetValueOrDefault();
                    var endTime = day.EndTime.GetValueOrDefault();
                    var hours = endTime - startTime;
                    totalHours += hours.TotalHours;
                }
                Assert.Equal(37, totalHours);
            }

            using var second = await client.PostAsJsonAsync(path, period);
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public async Task Admin_CanEndWorkSchedule_AndCreateNextPlan()
    {
        const string email = "schedule-admin@businessesbjerg.com";
        const string password = "ScheduleTestPassword123!";
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = CreateFactory(schema);
            using var client = CreateClient(factory);
            Guid employmentId;
            Guid scheduleId;
            WorkScheduleDay[] days;

            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create(email, password, "Admin");
                var employee = users.Create("schedule-employee@businessesbjerg.com", password, "Employee");
                var employments = scope.ServiceProvider.GetRequiredService<EmploymentService>();
                var employment = employments.Create(
                    employee.Id, EmploymentType.FullTime, new DateOnly(2026, 12, 1), null);
                Assert.NotNull(employment);
                employmentId = employment.Id;

                var schedules = scope.ServiceProvider.GetRequiredService<WorkScheduleService>();
                var plan = Assert.Single(schedules.GetByEmploymentId(employmentId));
                scheduleId = plan.Id;
                days = schedules.GetDays(scheduleId);
            }

            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var path = $"/api/employments/{employmentId}/work-schedules";
            var endPath = $"{path}/{scheduleId}/end";

            using var invalid = await client.PatchAsJsonAsync(endPath, new { endDate = "2026-11-30" });
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

            using var ended = await client.PatchAsJsonAsync(endPath, new { endDate = "2026-12-31" });
            Assert.Equal(HttpStatusCode.NoContent, ended.StatusCode);
            using var repeated = await client.PatchAsJsonAsync(endPath, new { endDate = "2026-12-31" });
            Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);

            using var created = await client.PostAsJsonAsync(
                path, new { startDate = "2027-01-01", days });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);

            var history = await client.GetFromJsonAsync<WorkSchedule[]>(path);
            Assert.NotNull(history);
            Assert.Equal(2, history.Length);
            Assert.Equal(new DateOnly(2026, 12, 31), history[1].EndDate);

            var decemberPlan = await client.GetFromJsonAsync<WorkSchedule>(
                $"{path}/for-date?date=2026-12-31");
            Assert.NotNull(decemberPlan);
            Assert.Equal(scheduleId, decemberPlan.Id);

            var januaryPlan = await client.GetFromJsonAsync<WorkSchedule>(
                $"{path}/for-date?date=2027-01-01");
            Assert.NotNull(januaryPlan);
            Assert.Equal(history[0].Id, januaryPlan.Id);

            using var noPlan = await client.GetAsync($"{path}/for-date?date=2026-11-30");
            Assert.Equal(HttpStatusCode.NotFound, noPlan.StatusCode);
            using var noDate = await client.GetAsync($"{path}/for-date");
            Assert.Equal(HttpStatusCode.BadRequest, noDate.StatusCode);

            var oldDays = await client.GetFromJsonAsync<WorkScheduleDay[]>($"{path}/{scheduleId}/days");
            Assert.NotNull(oldDays);
            Assert.Equal(5, oldDays.Length);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    internal static WebApplicationFactory<Program> CreateFactory(string schema)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<MyDatabaseConnection>();
                var options = new DataOptions()
                    .UsePostgreSQL(TestDatabase.ConnectionString(schema));
                services.AddScoped(_ => new MyDatabaseConnection(
                    new DataOptions<MyDatabaseConnection>(options)));
            });
        });
    }

    internal static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }
}
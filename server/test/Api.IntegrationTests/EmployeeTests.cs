using System.Net;
using System.Net.Http.Json;
using Infra;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace Api.IntegrationTests;

public class EmployeeTests
{
    [Fact]
    public async Task CreateEmployee_SavesEverythingOrNothing()
    {
        var schema = TestDatabase.CreateSchema();

        try
        {
            using var factory = LoginTests.CreateFactory(schema);
            using var client = LoginTests.CreateClient(factory);
            
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                users.Create("admin@example.test", "TestPassword123!", "Admin");
            }

            using var login = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "admin@example.test",
                password = "TestPassword123!"
            });

            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            
            using var invalid = await client.PostAsJsonAsync("/api/employees", new
            {
                email = "employee@businessesbjerg.com",
                password = "TestPassword123!",
                type = 2,
                startDate = "2026-12-01"
            });

            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                Assert.False(users.EmailExists("employee@businessesbjerg.com"));
            }
            
            using var created = await client.PostAsJsonAsync("/api/employees", new
            {
                email = "employee@businessesbjerg.com",
                password = "TestPassword123!",
                type = 1,
                startDate = "2026-12-01"
            });

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
                var employee = db.Users.Single(user =>
                    user.Email == "employee@businessesbjerg.com");

                var employment = db.Employments.Single(period =>
                    period.UserId == employee.Id);

                var plan = db.WorkSchedules.Single(schedule =>
                    schedule.EmploymentId == employment.Id);

                var numberOfDays = db.WorkScheduleDays.Count(day =>
                    day.WorkScheduleId == plan.Id);

                Assert.Equal(5, numberOfDays);
            }
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }
}
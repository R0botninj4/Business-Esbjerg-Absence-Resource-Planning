using System.Net;
using System.Net.Http.Json;
using Infra;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace Api.IntegrationTests;

public class UserEmailTests
{
    [Fact]
    public async Task EmployeeEmail_IsCheckedWhenCreatingAndUpdating()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            using var factory = LoginTests.CreateFactory(databasePath);
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

            // Forkert domæne skal give 400 og ingen ny konto.
            using var invalid = await client.PostAsJsonAsync("/api/users", new
            {
                email = "employee@gmail.com",
                password = "TestPassword123!",
                role = "Employee"
            });
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserService>();
                Assert.False(users.EmailExists("employee@gmail.com"));
            }

            using var created = await client.PostAsJsonAsync("/api/users", new
            {
                email = "EMPLOYEE@BUSINESSESBJERG.COM",
                password = "TestPassword123!",
                role = "Employee"
            });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            Assert.NotNull(created.Headers.Location);
            var path = created.Headers.Location.PathAndQuery;

            // En ugyldig ændring skal bevare den gamle e-mail.
            using var updated = await client.PutAsJsonAsync(path, new
            {
                email = "employee@gmail.com",
                role = "Employee"
            });
            Assert.Equal(HttpStatusCode.BadRequest, updated.StatusCode);
            var saved = await client.GetFromJsonAsync<User>(path);
            Assert.NotNull(saved);
            Assert.Equal("employee@businessesbjerg.com", saved.Email);

            // Reglen gælder også, når en regnskabskonto ændres til medarbejder.
            using var accounting = await client.PostAsJsonAsync("/api/users", new
            {
                email = "accounting@example.test",
                password = "TestPassword123!",
                role = "Accounting"
            });
            Assert.Equal(HttpStatusCode.Created, accounting.StatusCode);
            Assert.NotNull(accounting.Headers.Location);
            using var changedRole = await client.PutAsJsonAsync(accounting.Headers.Location, new
            {
                email = "accounting@example.test",
                role = "Employee"
            });
            Assert.Equal(HttpStatusCode.BadRequest, changedRole.StatusCode);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }
}

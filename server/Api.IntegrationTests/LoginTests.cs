using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.IntegrationTests;

public class LoginTests
{
    [Fact]
    public async Task Login_WithValidCredentials_GivesAccessToProfile()
    {
        const string email = "employee@example.test";
        const string password = "LocalTestPassword123!";
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<MyDatabaseConnection>();
                    var options = new DataOptions()
                        .UseSQLite($"Data Source={databasePath};Pooling=False");
                    services.AddScoped(_ => new MyDatabaseConnection(
                        new DataOptions<MyDatabaseConnection>(options)));
                });
            });

            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
                HandleCookies = true
            });

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
                var employee = new User { Id = Guid.NewGuid(), Email = email, Role = "Employee" };
                employee.PasswordHash = hasher.HashPassword(employee, password);
                db.Insert(employee);
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
            File.Delete(databasePath);
        }
    }
}
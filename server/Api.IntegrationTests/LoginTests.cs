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
            using var factory = CreateFactory(databasePath);
            using var client = CreateClient(factory);

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

    [Fact]
    public async Task Admin_CanManageUsers_AndDeletedUserLosesAccess()
    {
        const string adminEmail = "admin-crud@example.test";
        const string email = "employee-crud@example.test";
        const string password = "CrudTestPassword123!";
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            using var factory = CreateFactory(databasePath);
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
                scope.ServiceProvider.GetRequiredService<Service.UserService>()
                    .Create(adminEmail, password, "Admin");
            }

            using var adminLogin = await admin.PostAsJsonAsync(
                "/api/auth/login", new { email = adminEmail, password });
            Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);

            var request = new { email, password, role = "Employee" };
            using var created = await admin.PostAsJsonAsync("/api/users", request);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var body = await created.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(body.TryGetProperty("passwordHash", out _));
            var userPath = created.Headers.Location!.PathAndQuery;

            using var duplicate = await admin.PostAsJsonAsync("/api/users", request);
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

            using var listed = await admin.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
            var list = await listed.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Contains(list.EnumerateArray(), user => user.GetProperty("email").GetString() == email);

            using var updated = await admin.PutAsJsonAsync(userPath, new { email, role = "Accounting" });
            Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
            using var found = await admin.GetAsync(userPath);
            Assert.Equal(HttpStatusCode.OK, found.StatusCode);
            var profile = await found.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Accounting", profile.GetProperty("role").GetString());

            using var employeeLogin = await employee.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.OK, employeeLogin.StatusCode);
            using var forbidden = await employee.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

            using var deleted = await admin.DeleteAsync(userPath);
            Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
            using var missing = await admin.GetAsync(userPath);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
            using var expiredSession = await employee.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, expiredSession.StatusCode);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    private static WebApplicationFactory<Program> CreateFactory(string databasePath) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
}

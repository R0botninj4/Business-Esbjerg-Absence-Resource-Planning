using Infra;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Service;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
if (builder.Environment.IsDevelopment())
{
    var options = new DataOptions().UseSQLite("Data Source=business-esbjerg.db");
    var dbOptions = new DataOptions<MyDatabaseConnection>(options);

    builder.Services.AddScoped<MyDatabaseConnection>(
        _ => new MyDatabaseConnection(dbOptions));
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnValidatePrincipal = context =>
        {
            var principal = context.Principal;
            var user = Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? context.HttpContext.RequestServices.GetRequiredService<UserService>().Find(id)
                : null;

            if (user is null ||
                user.Email != principal?.FindFirstValue(ClaimTypes.Email) ||
                user.Role != principal?.FindFirstValue(ClaimTypes.Role))
            {
                context.RejectPrincipal();
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Business Esbjerg API");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
    db.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);
    db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_Users_Email ON Users (Email)");
    var users = scope.ServiceProvider.GetRequiredService<UserService>();

    SeedUser("Employee");
    SeedUser("Admin");

    void SeedUser(string role)
    {
        var email = app.Configuration[$"Seed{role}Email"];
        var password = app.Configuration[$"Seed{role}Password"];
        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(password) &&
            !users.EmailExists(email))
        {
            users.Create(email, password, role);
        }
    }
}


app.Run();
public partial class Program { }

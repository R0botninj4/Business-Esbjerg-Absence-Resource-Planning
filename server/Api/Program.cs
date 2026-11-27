using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Service;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
builder.Services.AddScoped<EmploymentService>();
builder.Services.AddScoped<WorkScheduleService>();
builder.Services.AddScoped<HolidayHoursService>();
builder.Services.AddScoped<HolidayRequestService>();
builder.Services.AddScoped<HolidayDecisionService>();
builder.Services.AddScoped<HolidayCancellationService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<ColleagueCalendarService>();
builder.Services.AddScoped<HolidayObstacleService>();
builder.Services.AddScoped<HolidayTransferService>();
builder.Services.AddScoped<HolidayRuleService>();
builder.Services.AddScoped<HolidayAccrualService>();
builder.Services.AddHostedService<Api.BackgroundServices.HolidayAccrualWorker>();
builder.Services.AddScoped<HolidayBalanceService>();
builder.Services.AddScoped<HolidayLedgerService>();
builder.Services.AddScoped<HolidayDebtService>();
builder.Services.AddScoped<HolidayConversionService>();
builder.Services.AddScoped<HolidayAdjustmentService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddOpenApi();
builder.Services.AddScoped<MyDatabaseConnection>(services =>
{
    var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString("Default");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Mangler forbindelse til databasen. Sæt miljøvariablen ConnectionStrings__Default.");
    }

    var options = new DataOptions().UsePostgreSQL(connectionString);
    return new MyDatabaseConnection(new DataOptions<MyDatabaseConnection>(options));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnValidatePrincipal = context =>
        {
            var principal = context.Principal;
            User? user = null;
            var userIdText = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdText, out var id))
            {
                var users = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                user = users.Find(id);
            }

            if (user is null ||
                !user.IsActive ||
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
using (var scope = app.Services.CreateScope())
{
    DatabaseMigrator.Migrate(scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>());
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var users = scope.ServiceProvider.GetRequiredService<UserService>();

    SeedUser("Employee");
    SeedUser("Accounting");
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

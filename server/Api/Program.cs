using Infra;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Service;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmploymentService>();
builder.Services.AddScoped<WorkScheduleService>();
builder.Services.AddScoped<HolidayRuleService>();
builder.Services.AddScoped<HolidayAccrualService>();
builder.Services.AddScoped<EmployeeService>();
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
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
    db.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);
    db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_Users_Email ON Users (Email)");
    db.CreateTable<Employment>(tableOptions: TableOptions.CreateIfNotExists);
    db.CreateTable<WorkSchedule>(tableOptions: TableOptions.CreateIfNotExists);
    db.CreateTable<WorkScheduleDay>(tableOptions: TableOptions.CreateIfNotExists);
    db.CreateTable<HolidayRule>(tableOptions: TableOptions.CreateIfNotExists);
    // Ældre regelversioner får tomme felter, så historiske satser ikke bliver gættet.
    if (db.Execute<int>("SELECT COUNT(*) FROM pragma_table_info('HolidayRules') WHERE name = 'HolidayDaysPerYear'") == 0)
    {
        db.Execute("ALTER TABLE HolidayRules ADD COLUMN HolidayDaysPerYear DECIMAL NULL");
    }

    if (db.Execute<int>("SELECT COUNT(*) FROM pragma_table_info('HolidayRules') WHERE name = 'SixthWeekHoursPerCalendarDay'") == 0)
    {
        db.Execute("ALTER TABLE HolidayRules ADD COLUMN SixthWeekHoursPerCalendarDay DECIMAL NULL");
    }

    db.CreateTable<HolidayAccrualMonth>(tableOptions: TableOptions.CreateIfNotExists);
    db.CreateTable<HolidayAccrualDay>(tableOptions: TableOptions.CreateIfNotExists);
    db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_AccrualMonths_User_Month ON HolidayAccrualMonths (UserId, Month)");
    db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_AccrualDays_Month_Date_Account ON HolidayAccrualDays (AccrualMonthId, Date, Account)");
    db.CreateTable<HolidayRulePeriodSettings>(tableOptions: TableOptions.CreateIfNotExists);
    db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_HolidayRules_Date_Version ON HolidayRules (EffectiveFrom, Version)");
    var users = scope.ServiceProvider.GetRequiredService<UserService>();

    var hasIsActiveColumn = db.Execute<int>(
        "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name = 'IsActive'") > 0;

    if (!hasIsActiveColumn)
    {
        db.Execute("ALTER TABLE Users ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1");
    }

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

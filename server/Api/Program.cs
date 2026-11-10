using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Service;

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
    .AddCookie();

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

    var email = app.Configuration["SeedEmployeeEmail"]?.Trim().ToLowerInvariant();
    var password = app.Configuration["SeedEmployeePassword"];

    if (!string.IsNullOrWhiteSpace(email) &&
        !string.IsNullOrWhiteSpace(password) &&
        !db.Users.Any(user => user.Email == email))
    {
        var employee = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = "Employee"
        };

        employee.PasswordHash = new PasswordHasher<User>()
            .HashPassword(employee, password);

        db.Insert(employee);
    }
}


app.Run();
public partial class Program { }

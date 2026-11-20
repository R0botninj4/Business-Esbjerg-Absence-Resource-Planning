using Infra;

namespace Service;

public class EmployeeService
{
    private readonly MyDatabaseConnection db;
    private readonly UserService users;
    private readonly EmploymentService employments;

    public EmployeeService(
        MyDatabaseConnection database,
        UserService userService,
        EmploymentService employmentService)
    {
        db = database;
        users = userService;
        employments = employmentService;
    }
    public User Create(
        string email,
        string password,
        EmploymentType type,
        DateOnly startDate,
        DateOnly? endDate,
        WorkScheduleDayRequest[]? days)
    {
        using var transaction = db.BeginTransaction();

        var user = users.Create(email, password, "Employee");

        var employment = employments.CreateInTransaction(
            user.Id, type, startDate, endDate, days);

        if (employment == null)
        {
            throw new InvalidOperationException(
                "Ansættelsen kunne ikke oprettes.");
        }

        transaction.Commit();

        return user;
    }
}
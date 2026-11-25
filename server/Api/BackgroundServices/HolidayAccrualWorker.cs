using Infra;
using Service;

namespace Api.BackgroundServices;

public class HolidayAccrualWorker : BackgroundService
{
    private readonly IServiceScopeFactory scopes;
    private readonly ILogger<HolidayAccrualWorker> logger;

    public HolidayAccrualWorker(IServiceScopeFactory scopeFactory, ILogger<HolidayAccrualWorker> log)
    {
        scopes = scopeFactory;
        logger = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                PostMissingMonths(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Automatisk ferieoptjening kunne ikke køre.");
            }

            // Nye regler og ugeplaner bliver opdaget uden en genstart.
            // Gemte måneder genbruges, så timerne ikke bliver tilføjet igen.
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private void PostMissingMonths(CancellationToken stoppingToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
        var userIds = db.Employments
            .Where(employment => employment.Type != EmploymentType.Hourly)
            .Select(employment => employment.UserId)
            .Distinct()
            .ToArray();

        foreach (var userId in userIds)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                // Hver medarbejder får sin egen databaseforbindelse.
                using var employeeScope = scopes.CreateScope();
                var accrual = employeeScope.ServiceProvider.GetRequiredService<HolidayAccrualService>();
                accrual.PostMissingMonths(userId);
            }
            catch (Exception exception)
            {
                // Manglende regler eller arbejdsplan må ikke stoppe de andre.
                logger.LogError(exception, "Kunne ikke bogføre ferie for {UserId}.", userId);
            }
        }
    }
}

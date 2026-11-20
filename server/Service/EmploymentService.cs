using Infra;
using LinqToDB;

namespace Service;

public class EmploymentService
{
    private readonly MyDatabaseConnection db;
    private readonly WorkScheduleService workSchedules;

    public EmploymentService(MyDatabaseConnection database, WorkScheduleService schedules)
    {
        db = database;
        workSchedules = schedules;
    }

    public Employment[] GetByUserId(Guid userId)
    {
        return db.Employments
            .Where(employment => employment.UserId == userId)
            .OrderByDescending(employment => employment.StartDate)
            .ToArray();
    }
    public Employment? Find(Guid userId, Guid employmentId)
    {
        return db.Employments.SingleOrDefault(period =>
            period.UserId == userId && period.Id == employmentId);
    }
    public bool End(Guid userId, Guid employmentId, DateOnly endDate)
    {
        using var transaction = db.BeginTransaction();
        var hasLaterPlan = db.WorkSchedules.Any(schedule =>
            schedule.EmploymentId == employmentId && schedule.StartDate > endDate);

        if (hasLaterPlan)
        {
            return false;
        }

        var updatedRows = db.Employments
            .Where(period => period.UserId == userId &&
                             period.Id == employmentId &&
                             period.EndDate == null &&
                             period.StartDate <= endDate)
            .Set(period => period.EndDate, (DateOnly?)endDate)
            .Update();

        if (updatedRows == 0)
        {
            return false;
        }

        // Ugeplanerne må ikke fortsætte efter ansættelsens slutdato.
        db.WorkSchedules
            .Where(schedule => schedule.EmploymentId == employmentId &&
                               (schedule.EndDate == null || schedule.EndDate > endDate))
            .Set(schedule => schedule.EndDate, (DateOnly?)endDate)
            .Update();

        transaction.Commit();
        return updatedRows == 1;
    }
    public Employment? Create(
        Guid userId, EmploymentType type, DateOnly startDate, DateOnly? endDate,
        WorkScheduleDayRequest[]? days = null)
    {
        using var transaction = db.BeginTransaction();

        var employment = CreateInTransaction(
            userId, type, startDate, endDate, days);

        transaction.Commit();

        return employment;
    }

    internal Employment? CreateInTransaction(
        Guid userId, EmploymentType type, DateOnly startDate, DateOnly? endDate,
        WorkScheduleDayRequest[]? days = null)
    {
        ValidateDates(startDate, endDate);
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException("Vælg en gyldig ansættelsestype.");
        }

        if (type == EmploymentType.PartTime && days == null)
        {
            throw new ArgumentException("Ugeplanen skal indeholde fem hverdage.");
        }

        if (days != null)
        {
            WorkScheduleService.ValidateDays(days);
        }

        var isActiveEmployee = db.Users.Any(user =>
            user.Id == userId && user.IsActive && user.Role == "Employee");

        if (!isActiveEmployee)
        {
            return null;
        }

        var newEndDate = DateOnly.MaxValue;
        if (endDate.HasValue)
        {
            newEndDate = endDate.Value;
        }

        var hasOverlap = db.Employments.Any(period =>
            period.UserId == userId &&
            period.StartDate <= newEndDate &&
            (period.EndDate == null || period.EndDate >= startDate));

        if (hasOverlap)
        {
            throw new EmploymentOverlapException();
        }

        var employment = new Employment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            StartDate = startDate,
            EndDate = endDate
        };

        db.Insert(employment);

        if (days != null)
        {
            workSchedules.CreateInitial(employment, days);
        }
        else if (type == EmploymentType.FullTime)
        {
            // Fuldtid bruger standardtiderne, når der ikke er sendt en plan med.
            workSchedules.CreateFullTime(employment);
        }

        return employment;
    }

    public static void ValidateDates(DateOnly startDate, DateOnly? endDate)
    {
        if (startDate == default)
        {
            throw new ArgumentException("Angiv en startdato.", nameof(startDate));
        }

        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new ArgumentException(
                "Slutdatoen må ikke være før startdatoen.", nameof(endDate));
        }
    }
}

public class EmploymentOverlapException()
    : Exception("Ansættelsesperioden overlapper en eksisterende periode.")
{
}

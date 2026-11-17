using Infra;
using LinqToDB;

namespace Service;

public class EmploymentService(MyDatabaseConnection db)
{
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
        var updatedRows = db.Employments
            .Where(period => period.UserId == userId &&
                             period.Id == employmentId &&
                             period.EndDate == null &&
                             period.StartDate <= endDate)
            .Set(period => period.EndDate, (DateOnly?)endDate)
            .Update();

        return updatedRows == 1;
    }
    public Employment? Create(
        Guid userId, EmploymentType type, DateOnly startDate, DateOnly? endDate)
    {
        ValidateDates(startDate, endDate);

        var isActiveEmployee = db.Users.Any(user =>
            user.Id == userId && user.IsActive && user.Role == "Employee");

        if (!isActiveEmployee)
        {
            return null;
        }

        var newEndDate = endDate ?? DateOnly.MaxValue;

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

using Infra;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;

namespace Service;

public class WorkScheduleService
{
    private readonly MyDatabaseConnection db;
    private readonly IConfiguration configuration;
    private readonly HolidayDebtService debt;
    private readonly HolidayConversionService conversions;

    public WorkScheduleService(MyDatabaseConnection database, IConfiguration settings,
        HolidayDebtService debtService, HolidayConversionService conversionService)
    {
        db = database;
        configuration = settings;
        debt = debtService;
        conversions = conversionService;
    }

    public WorkSchedule[] GetByEmploymentId(Guid employmentId)
    {
        return db.WorkSchedules
            .Where(schedule => schedule.EmploymentId == employmentId)
            .OrderByDescending(schedule => schedule.StartDate)
            .ToArray();
    }

    public WorkScheduleDay[] GetDays(Guid workScheduleId)
    {
        return db.WorkScheduleDays
            .Where(day => day.WorkScheduleId == workScheduleId)
            .OrderBy(day => day.Day)
            .ToArray();
    }

    public Employment? FindEmployment(Guid employmentId)
    {
        return db.Employments.SingleOrDefault(employment =>
            employment.Id == employmentId);
    }

    public WorkSchedule? Find(Guid employmentId, Guid workScheduleId)
    {
        return db.WorkSchedules.SingleOrDefault(schedule =>
            schedule.EmploymentId == employmentId && schedule.Id == workScheduleId);
    }

    public WorkSchedule? GetForDate(Guid employmentId, DateOnly date)
    {
        // Begge grænsedatoer tæller med. En tom slutdato betyder, at planen fortsætter.
        return db.WorkSchedules.SingleOrDefault(schedule =>
            schedule.EmploymentId == employmentId &&
            schedule.StartDate <= date &&
            (schedule.EndDate == null || schedule.EndDate >= date));
    }

    public bool End(Guid employmentId, Guid workScheduleId, DateOnly endDate)
    {
        using var transaction = db.BeginTransaction();
        var employment = FindEmployment(employmentId);
        if (employment == null)
        {
            return false;
        }
        LockUser(employment.UserId);
        var updatedRows = db.WorkSchedules
            .Where(schedule => schedule.EmploymentId == employmentId &&
                               schedule.Id == workScheduleId &&
                               schedule.EndDate == null &&
                               schedule.StartDate <= endDate)
            .Set(schedule => schedule.EndDate, (DateOnly?)endDate)
            .Update();

        debt.ValidateExisting(employment.UserId);
        conversions.Record(employment.UserId);
        transaction.Commit();
        return updatedRows == 1;
    }

    public static void ValidatePeriod(
        Employment employment, DateOnly startDate, DateOnly? endDate)
    {
        if (startDate < employment.StartDate)
        {
            throw new ArgumentException(
                "Ugeplanen må ikke starte før ansættelsen.");
        }

        if (endDate.HasValue)
        {
            if (endDate.Value < startDate)
            {
                throw new ArgumentException("Slutdatoen må ikke være før startdatoen.");
            }
        }

        if (employment.EndDate.HasValue)
        {
            if (!endDate.HasValue)
            {
                throw new ArgumentException(
                    "Ugeplanen skal slutte senest på ansættelsens slutdato.");
            }

            if (endDate.Value > employment.EndDate.Value)
            {
                throw new ArgumentException(
                    "Ugeplanen skal slutte senest på ansættelsens slutdato.");
            }
        }
    }

    public WorkSchedule? Create(
        Guid employmentId,
        DateOnly startDate,
        DateOnly? endDate,
        WorkScheduleDayRequest[] days)
    {
        var employment = FindEmployment(employmentId);

        if (employment == null)
        {
            return null;
        }

        ValidatePeriod(employment, startDate, endDate);
        ValidateDays(days);

        // Gem planen og alle fem dage samlet.
        using var transaction = db.BeginTransaction();

        LockUser(employment.UserId);

        if (HasOverlap(employmentId, startDate, endDate))
        {
            throw new WorkScheduleOverlapException();
        }

        var schedule = Save(employmentId, startDate, endDate, days);
        debt.ValidateExisting(employment.UserId);
        conversions.Record(employment.UserId);
        transaction.Commit();

        return schedule;
    }

    public WorkSchedule? Change(
        Guid employmentId, Guid workScheduleId, DateOnly startDate,
        WorkScheduleDayRequest[] days)
    {
        using var transaction = db.BeginTransaction();
        var oldPlan = Find(employmentId, workScheduleId);
        if (oldPlan == null)
        {
            return null;
        }

        if (startDate <= oldPlan.StartDate)
        {
            throw new ArgumentException("Ændringen skal starte efter planens start og inden for dens periode.");
        }

        if (oldPlan.EndDate.HasValue)
        {
            if (startDate > oldPlan.EndDate.Value)
            {
                throw new ArgumentException("Ændringen skal starte efter planens start og inden for dens periode.");
            }
        }

        var employment = FindEmployment(employmentId);
        if (employment == null)
        {
            return null;
        }

        ValidatePeriod(employment, startDate, oldPlan.EndDate);
        ValidateDays(days);
        LockUser(employment.UserId);

        // Den gamle version slutter dagen før den nye starter.
        var oldEndDate = startDate.AddDays(-1);
        db.WorkSchedules.Where(schedule => schedule.Id == workScheduleId)
            .Set(schedule => schedule.EndDate, (DateOnly?)oldEndDate)
            .Update();

        if (HasOverlap(employmentId, startDate, oldPlan.EndDate))
        {
            throw new WorkScheduleOverlapException();
        }

        var newPlan = Save(employmentId, startDate, oldPlan.EndDate, days);
        debt.ValidateExisting(employment.UserId);
        conversions.Record(employment.UserId);
        transaction.Commit();

        return newPlan;
    }

    internal void CreateInitial(Employment employment, WorkScheduleDayRequest[] days)
    {
        // EmploymentService har allerede startet en transaktion.
        ValidateDays(days);
        Save(employment.Id, employment.StartDate, employment.EndDate, days);
        debt.ValidateExisting(employment.UserId);
        conversions.Record(employment.UserId);
    }

    private void LockUser(Guid userId)
    {
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
    }

    internal void CreateFullTime(Employment employment)
    {
        var section = configuration.GetSection("FullTimeWorkSchedule");
        var days = section.Get<WorkScheduleDayRequest[]>();

        if (days == null)
        {
            days = new WorkScheduleDayRequest[0];
        }

        CreateInitial(employment, days);
    }

    private WorkSchedule Save(
        Guid employmentId, DateOnly startDate, DateOnly? endDate,
        WorkScheduleDayRequest[] days)
    {
        var schedule = new WorkSchedule
        {
            Id = Guid.NewGuid(),
            EmploymentId = employmentId,
            StartDate = startDate,
            EndDate = endDate
        };

        db.Insert(schedule);

        foreach (var day in days)
        {
            var scheduleDay = new WorkScheduleDay
            {
                Id = Guid.NewGuid(),
                WorkScheduleId = schedule.Id,
                Day = day.Day,
                IsDayOff = day.IsDayOff,
                StartTime = day.StartTime,
                EndTime = day.EndTime
            };

            db.Insert(scheduleDay);
        }

        return schedule;
    }

    public bool HasOverlap(
        Guid employmentId, DateOnly startDate, DateOnly? endDate)
    {
        var lastDate = DateOnly.MaxValue;
        if (endDate.HasValue)
        {
            lastDate = endDate.Value;
        }

        return db.WorkSchedules.Any(schedule =>
            schedule.EmploymentId == employmentId &&
            schedule.StartDate <= lastDate &&
            (schedule.EndDate == null || schedule.EndDate >= startDate));
    }

    public static void ValidateDays(WorkScheduleDayRequest[] days)
    {
        if (days == null || days.Length != 5)
        {
            throw new ArgumentException("Ugeplanen skal indeholde fem hverdage.");
        }

        foreach (var day in days)
        {
            if (day == null)
            {
                throw new ArgumentException("Alle fem hverdage skal være udfyldt.");
            }
        }

        // DayOfWeek bruger 1 for mandag og 5 for fredag.
        for (int dayNumber = 1; dayNumber <= 5; dayNumber++)
        {
            var weekday = (DayOfWeek)dayNumber;
            int count = 0;

            foreach (var day in days)
            {
                if (day.Day == weekday)
                {
                    count++;
                }
            }

            if (count != 1)
            {
                throw new ArgumentException(
                    "Mandag til fredag skal hver optræde én gang.");
            }
        }

        foreach (var day in days)
        {
            if (day.IsDayOff)
            {
                if (day.StartTime.HasValue || day.EndTime.HasValue)
                {
                    throw new ArgumentException("En fridag skal være uden arbejdstider.");
                }

            }
            else
            {
                if (!day.StartTime.HasValue || !day.EndTime.HasValue)
                {
                    throw new ArgumentException("En arbejdsdag skal have start- og sluttid.");
                }

                if (day.EndTime.Value <= day.StartTime.Value)
                {
                    throw new ArgumentException("Sluttiden skal være efter starttiden.");
                }
            }
        }
    }
}

public class WorkScheduleOverlapException : Exception
{
    public WorkScheduleOverlapException()
        : base("Ugeplanen overlapper en eksisterende plan.")
    {
    }
}

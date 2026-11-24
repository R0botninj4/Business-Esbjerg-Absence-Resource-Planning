using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayRequestService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayHoursService hours;
    private readonly HolidayRuleService rules;

    public HolidayRequestService(MyDatabaseConnection database, HolidayHoursService hourService, HolidayRuleService ruleService)
    {
        db = database;
        hours = hourService;
        rules = ruleService;
    }

    public HolidayRequest[] GetOwn(Guid userId)
    {
        return db.HolidayRequests.Where(request => request.UserId == userId)
            .OrderByDescending(request => request.CreatedAt).ToArray();
    }

    public HolidayRequest[] GetPending()
    {
        return db.HolidayRequests.Where(request => request.Status == "pending")
            .OrderBy(request => request.CreatedAt).ToArray();
    }

    public HolidayRequest[] GetForAccounting()
    {
        return db.HolidayRequests.OrderBy(request => request.CreatedAt).ToArray();
    }

    public HolidayRequest? FindOwn(Guid userId, Guid requestId)
    {
        return db.HolidayRequests.SingleOrDefault(request => request.Id == requestId && request.UserId == userId);
    }

    public HolidayRequest? Find(Guid requestId)
    {
        return db.HolidayRequests.SingleOrDefault(request => request.Id == requestId);
    }

    public HolidayRequestDay[] GetDays(Guid requestId)
    {
        return db.HolidayRequestDays.Where(day => day.RequestId == requestId)
            .OrderBy(day => day.Date).ToArray();
    }

    public HolidayRequestPreview Preview(Guid userId, HolidayRequestInput input, Guid? ignoredRequestId = null)
    {
        ValidateDates(input);
        CheckOverlap(userId, input, ignoredRequestId);
        var result = new HolidayRequestPreview();
        var employments = db.Employments.Where(employment => employment.UserId == userId).ToArray();
        var date = input.StartDate;
        while (true)
        {
            var day = CalculateDay(employments, input.Account, date);
            result.Days.Add(day);
            result.Hours += day.Hours;
            if (date == input.EndDate)
            {
                break;
            }
            date = date.AddDays(1);
        }
        result.Hours = Math.Round(result.Hours, 2);
        if (result.Hours <= 0)
        {
            throw new ArgumentException("Perioden indeholder ingen arbejdstimer, der kan søges ferie til.");
        }
        return result;
    }

    private HolidayRequestDay CalculateDay(Employment[] employments, HolidayAccount account, DateOnly date)
    {
        var employment = employments.SingleOrDefault(period =>
            period.StartDate <= date && (period.EndDate == null || period.EndDate >= date));
        if (employment == null)
        {
            throw new ArgumentException("Der mangler en ansættelse for " + date + ".");
        }
        if (employment.Type == EmploymentType.Hourly)
        {
            throw new ArgumentException("Timelønnede bruger ikke disse feriekonti.");
        }
        var rule = rules.GetForDate(date);
        if (rule == null || rule.Periods == null)
        {
            throw new ArgumentException("Systemejeren skal oprette komplette ferieregler for " + date + ".");
        }
        if (account == HolidayAccount.SixthWeek && !rule.SixthWeekEnabled)
        {
            throw new ArgumentException("6. ferieuge er ikke aktiveret for " + date + ".");
        }
        CheckUsagePeriod(rule.Periods, account, date);

        var calculated = hours.Calculate(employment.Id, date, date).Days[0];
        var day = new HolidayRequestDay();
        day.Date = date;
        day.Hours = calculated.Hours;
        day.Description = calculated.Description;
        day.EmploymentId = employment.Id;
        day.HolidayRuleId = rule.Id;
        day.WorkScheduleId = calculated.WorkScheduleId;
        day.CalendarDayId = calculated.CalendarDayId;
        return day;
    }

    public HolidayRequest Create(Guid userId, HolidayRequestInput input)
    {
        using var transaction = db.BeginTransaction();
        // Samtidige indsendelser for samme medarbejder venter på hinanden.
        // Derfor kan begge kald ikke slippe forbi overlapkontrollen samtidigt.
        var lockedUser = db.Query<Guid>("SELECT id FROM users WHERE id=@id AND is_active=true AND role='Employee' FOR UPDATE",
            new DataParameter("id", userId)).SingleOrDefault();
        if (lockedUser == Guid.Empty)
        {
            throw new ArgumentException("Medarbejderkontoen er ikke aktiv.");
        }
        var preview = Preview(userId, input);
        var request = new HolidayRequest();
        request.Id = Guid.NewGuid();
        request.UserId = userId;
        request.Account = input.Account;
        request.StartDate = input.StartDate;
        request.EndDate = input.EndDate;
        request.Hours = preview.Hours;
        request.CreatedAt = DateTime.UtcNow;
        db.Insert(request);
        foreach (var day in preview.Days)
        {
            day.Id = Guid.NewGuid();
            day.RequestId = request.Id;
            db.Insert(day);
        }
        transaction.Commit();
        return request;
    }

    public bool Withdraw(Guid userId, Guid requestId)
    {
        // Ejerskab og status kontrolleres i selve UPDATE, så en behandlet ansøgning ikke kan trækkes tilbage.
        int changed = db.HolidayRequests.Where(request => request.Id == requestId && request.UserId == userId && request.Status == "pending")
            .Set(request => request.Status, "cancelled")
            .Set(request => request.WithdrawnAt, (DateTime?)DateTime.UtcNow).Update();
        if (changed == 1)
        {
            return true;
        }
        var existing = FindOwn(userId, requestId);
        if (existing == null)
        {
            return false;
        }
        if (existing.Status == "cancelled")
        {
            return true;
        }
        throw new HolidayRequestConflictException("Kun afventende ansøgninger kan trækkes tilbage.");
    }

    private static void ValidateDates(HolidayRequestInput input)
    {
        if (!Enum.IsDefined(input.Account))
        {
            throw new ArgumentException("Vælg uge 1–5 eller 6. ferieuge.");
        }
        if (input.StartDate == default || input.EndDate == default)
        {
            throw new ArgumentException("Angiv både start- og slutdato.");
        }
        if (input.StartDate < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Der kan ikke søges ferie i fortiden.");
        }
        if (input.EndDate < input.StartDate)
        {
            throw new ArgumentException("Slutdatoen må ikke være før startdatoen.");
        }
    }

    private void CheckOverlap(Guid userId, HolidayRequestInput input, Guid? ignoredRequestId)
    {
        bool overlaps = db.HolidayRequests.Any(request => request.UserId == userId &&
            request.Id != ignoredRequestId &&
            (request.Status == "pending" || request.Status == "approved") &&
            request.StartDate <= input.EndDate && request.EndDate >= input.StartDate);
        if (overlaps)
        {
            throw new HolidayRequestConflictException("Perioden overlapper en afventende eller godkendt ansøgning.");
        }
    }

    private static void CheckUsagePeriod(HolidayRulePeriodSettings settings, HolidayAccount account, DateOnly date)
    {
        // Find de mulige optjeningsår ud fra reglens årsforskydninger, ikke faste kalenderår.
        int startOffset = settings.HolidayUsageStartYearOffset;
        int endOffset = settings.HolidayUsageEndYearOffset;
        if (account == HolidayAccount.SixthWeek)
        {
            startOffset = settings.SixthWeekUsageStartYearOffset;
            endOffset = settings.SixthWeekUsageEndYearOffset;
        }
        for (int year = Math.Max(1, date.Year - endOffset); year <= date.Year - startOffset; year++)
        {
            var periods = HolidayRulePeriodsCalculator.Calculate(settings, year);
            var period = periods.WeeksOneToFive;
            if (account == HolidayAccount.SixthWeek)
            {
                period = periods.SixthWeek;
            }
            if (date >= period.UsageStart && date <= period.UsageEnd) return;
        }
        throw new ArgumentException("Datoen " + date + " ligger uden for kontoens afholdelsesperiode.");
    }
}

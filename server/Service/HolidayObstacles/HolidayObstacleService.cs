using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayObstacleService
{
    private readonly MyDatabaseConnection db;

    public HolidayObstacleService(MyDatabaseConnection database)
    {
        db = database;
    }

    public HolidayObstacle[] List(Guid userId, bool accounting)
    {
        if (accounting) return db.HolidayObstacles.OrderBy(item => item.CreatedAt).ToArray();
        return db.HolidayObstacles.Where(item => item.UserId == userId).OrderBy(item => item.CreatedAt).ToArray();
    }

    public HolidayObstacleResult? Get(Guid id, Guid userId, bool accounting)
    {
        var report = db.HolidayObstacles.SingleOrDefault(item => item.Id == id);
        if (report == null) return null;
        if (!accounting && report.UserId != userId) return null;
        var result = new HolidayObstacleResult();
        result.Report = report;
        result.Days = db.HolidayObstacleDays.Where(day => day.ObstacleId == id).OrderBy(day => day.Date).ToList();
        return result;
    }

    public HolidayObstacle Create(Guid userId, HolidayObstacleInput input)
    {
        if (input.OperationId == Guid.Empty) throw new ArgumentException("Angiv operations-ID.");
        if (input.StartDate == default || input.EndDate < input.StartDate || input.EndDate.DayNumber - input.StartDate.DayNumber > 365)
            throw new ArgumentException("Angiv en gyldig periode på højst 366 dage.");
        if (input.DocumentationReference == null || input.DocumentationReference.Length > 500)
            throw new ArgumentException("Dokumentationsreferencen må højst være 500 tegn.");
        if (input.Kind != "sicknessBefore" && input.Kind != "sicknessDuring" && input.Kind != "parentalLeave" &&
            input.Kind != "strike" && input.Kind != "other") throw new ArgumentException("Vælg en gyldig feriehindring.");

        using var transaction = db.BeginTransaction();
        LockUser(userId);
        var existing = db.HolidayObstacles.SingleOrDefault(item => item.OperationId == input.OperationId);
        if (existing != null)
        {
            if (existing.UserId != userId || existing.RequestId != input.RequestId || existing.Kind != input.Kind ||
                existing.StartDate != input.StartDate || existing.EndDate != input.EndDate ||
                existing.DocumentationReference != input.DocumentationReference.Trim())
                throw new HolidayRequestConflictException("Operations-ID'et er allerede brugt til andet indhold.");
            return existing;
        }
        bool overlaps = db.HolidayObstacles.Any(item => item.UserId == userId && item.Status != "declined" &&
            item.StartDate <= input.EndDate && item.EndDate >= input.StartDate);
        if (overlaps) throw new HolidayRequestConflictException("Perioden overlapper en eksisterende feriehindring.");
        if (!db.Employments.Any(item => item.UserId == userId && item.Type != EmploymentType.Hourly &&
            item.StartDate <= input.EndDate && (item.EndDate == null || item.EndDate >= input.StartDate)))
            throw new ArgumentException("Feriehindringen skal overlappe en fuldtids- eller deltidsansættelse.");
        var report = new HolidayObstacle();
        report.Id = Guid.NewGuid();
        report.OperationId = input.OperationId;
        report.UserId = userId;
        report.RequestId = input.RequestId;
        report.Kind = input.Kind;
        report.StartDate = input.StartDate;
        report.EndDate = input.EndDate;
        report.DocumentationReference = input.DocumentationReference.Trim();
        report.CreatedAt = DateTime.UtcNow;

        var days = new List<HolidayObstacleDay>();
        if (input.RequestId.HasValue)
        {
            days = ReadHolidayDays(report);
        }
        else if (input.Kind == "sicknessBefore" || input.Kind == "sicknessDuring")
        {
            throw new ArgumentException("Sygdom før eller under ferie skal knyttes til en godkendt ferie.");
        }
        db.Insert(report);
        foreach (var day in days) db.Insert(day);
        transaction.Commit();
        return report;
    }

    private List<HolidayObstacleDay> ReadHolidayDays(HolidayObstacle report)
    {
        var request = db.HolidayRequests.SingleOrDefault(item => item.Id == report.RequestId && item.UserId == report.UserId);
        if (request == null || request.Status != "approved") throw new ArgumentException("Vælg din egen godkendte ferie.");
        if (report.Kind == "sicknessBefore" && report.StartDate >= request.StartDate)
            throw new ArgumentException("Sygdommen skal være startet før ferien.");
        if (report.Kind == "sicknessDuring" && report.StartDate < request.StartDate)
            throw new ArgumentException("Sygdom under ferien må ikke starte før ferien.");

        var requestDays = db.HolidayRequestDays.Where(day => day.RequestId == request.Id && day.Basis == "approval").ToArray();
        var dayIds = requestDays.Select(day => day.Id).ToArray();
        var usage = db.HolidayUsage.Where(item => dayIds.Contains(item.RequestDayId) && item.CancelledAt == null &&
            item.Date >= report.StartDate && item.Date <= report.EndDate).OrderBy(item => item.Date).ToArray();
        if (usage.Length == 0) throw new ArgumentException("Perioden indeholder intet aktivt ferieforbrug.");

        var result = new List<HolidayObstacleDay>();
        foreach (var item in usage)
        {
            var requestDay = requestDays.Single(day => day.Id == item.RequestDayId);
            var rule = db.HolidayRules.Single(rule => rule.Id == requestDay.HolidayRuleId);
            var settings = db.HolidayRulePeriods.Single(settings => settings.HolidayRuleId == rule.Id);
            int year = item.Date.Year;
            var yearStart = new DateOnly(year, settings.HolidayEarningStartMonth, settings.HolidayEarningStartDay);
            if (item.Date < yearStart) yearStart = yearStart.AddYears(-1);
            var day = new HolidayObstacleDay();
            day.Id = Guid.NewGuid();
            day.ObstacleId = report.Id;
            day.UsageId = item.Id;
            day.Date = item.Date;
            day.Hours = item.Hours;
            day.HolidayYearStart = yearStart;
            day.WaitingDays = rule.SicknessWaitingDays;
            result.Add(day);
        }
        return result;
    }

    public HolidayObstacle? Decide(Guid id, Guid decidedBy, HolidayDecisionInput input)
    {
        string comment = (input.Comment ?? "").Trim();
        if (comment.Length > 500) throw new ArgumentException("Kommentaren må højst være 500 tegn.");
        if (comment.Length == 0) throw new ArgumentException("Skriv en begrundelse for afgørelsen.");
        using var transaction = db.BeginTransaction();
        var report = db.HolidayObstacles.SingleOrDefault(item => item.Id == id);
        if (report == null) return null;
        LockUser(report.UserId);
        report = db.HolidayObstacles.Single(item => item.Id == id);
        string status = "declined";
        if (input.Approve) status = "approved";
        if (report.Status != "pending")
        {
            if (report.Status == status && report.DecidedBy == decidedBy && report.Comment == comment) return report;
            throw new HolidayRequestConflictException("Feriehindringen er allerede behandlet.");
        }
        if (input.Approve) Refund(report);
        report.Status = status;
        report.DecidedAt = DateTime.UtcNow;
        report.DecidedBy = decidedBy;
        report.Comment = comment;
        db.Update(report);
        transaction.Commit();
        return report;
    }

    private void Refund(HolidayObstacle report)
    {
        var days = db.HolidayObstacleDays.Where(day => day.ObstacleId == report.Id).OrderBy(day => day.Date).ToArray();
        var priorReports = db.HolidayObstacles.Where(item => item.UserId == report.UserId &&
            item.Status == "approved" && item.Kind == "sicknessDuring").Select(item => item.Id).ToArray();
        var priorDays = db.HolidayObstacleDays.Where(day => priorReports.Contains(day.ObstacleId) &&
            db.HolidayUsage.Any(usage => usage.Id == day.UsageId && usage.Account == HolidayAccount.WeeksOneToFive)).ToArray();
        var waitingDates = new List<DateOnly>();

        foreach (var day in days)
        {
            var usage = db.HolidayUsage.Single(item => item.Id == day.UsageId);
            if (usage.CancelledAt.HasValue) throw new HolidayRequestConflictException("Ferietimerne er allerede frigivet.");
            bool waitingRequired = report.Kind == "sicknessDuring" && usage.Account == HolidayAccount.WeeksOneToFive;
            if (waitingRequired)
            {
                if (report.DocumentationReference.Length == 0) throw new ArgumentException("Sygdom under uge 1–5 kræver en dokumentationsreference.");
                var earlier = priorDays.Where(item => item.HolidayYearStart == day.HolidayYearStart).ToArray();
                if (earlier.Any(item => item.Date >= day.Date))
                    throw new HolidayRequestConflictException("Behandl sygdomsperioderne i datoorden inden for samme ferieår.");
                int used = earlier.Select(item => item.Date).Distinct().Count();
                used += waitingDates.Count(date => date >= day.HolidayYearStart && date < day.Date);
                if (used < day.WaitingDays)
                {
                    day.IsWaitingDay = true;
                    if (!waitingDates.Contains(day.Date)) waitingDates.Add(day.Date);
                    db.Update(day);
                    continue;
                }
            }
            // Frigiv hele arbejdsdage på deres oprindelige konto. Behold originalposterne.
            usage.CancelledAt = DateTime.UtcNow;
            db.Update(usage);
            day.RefundedHours = day.Hours;
            db.Update(day);
        }
    }

    private void LockUser(Guid userId)
    {
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
    }
}

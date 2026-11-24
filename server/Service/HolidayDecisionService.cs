using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayDecisionInput
{
    public bool Approve { get; set; }
    public string Comment { get; set; } = "";
}

public class HolidayDecisionService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayRequestService requests;
    private readonly HolidayBalanceService balances;

    public HolidayDecisionService(MyDatabaseConnection database, HolidayRequestService requestService,
        HolidayBalanceService balanceService)
    {
        db = database;
        requests = requestService;
        balances = balanceService;
    }

    public HolidayRequest? Decide(Guid requestId, Guid decidedBy, HolidayDecisionInput input)
    {
        string comment = (input.Comment ?? "").Trim();
        if (comment.Length > 500)
        {
            throw new ArgumentException("Kommentaren må højst være 500 tegn.");
        }
        if (!input.Approve && comment.Length == 0)
        {
            throw new ArgumentException("Skriv en begrundelse for afslaget.");
        }

        using var transaction = db.BeginTransaction();
        var request = db.HolidayRequests.SingleOrDefault(item => item.Id == requestId);
        if (request == null)
        {
            return null;
        }

        // Alle afgørelser for samme medarbejder venter på hinanden.
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE",
            new DataParameter("id", request.UserId)).ToArray();
        // Tilbagetrækning venter også, indtil afgørelsen er gemt.
        db.Query<Guid>("SELECT id FROM holiday_requests WHERE id=@id FOR UPDATE",
            new DataParameter("id", requestId)).ToArray();
        request = db.HolidayRequests.Single(item => item.Id == requestId);

        string status = "declined";
        if (input.Approve)
        {
            status = "approved";
        }
        if (request.Status != "pending")
        {
            // Samme afgørelse kan gentages uden at bogføre timer igen.
            if (request.Status == status && request.DecidedBy == decidedBy && request.DecisionComment == comment)
            {
                return request;
            }
            throw new HolidayRequestConflictException("Ansøgningen er allerede behandlet eller trukket tilbage.");
        }

        if (input.Approve)
        {
            if (!db.Users.Any(user => user.Id == request.UserId && user.IsActive && user.Role == "Employee"))
            {
                throw new HolidayRequestConflictException("Medarbejderkontoen er ikke aktiv.");
            }
            Approve(request);
        }
        request.Status = status;
        request.DecidedAt = DateTime.UtcNow;
        request.DecidedBy = decidedBy;
        request.DecisionComment = comment;
        db.Update(request);
        transaction.Commit();
        return request;
    }

    private void Approve(HolidayRequest request)
    {
        var input = new HolidayRequestInput();
        input.Account = request.Account;
        input.StartDate = request.StartDate;
        input.EndDate = request.EndDate;
        // Genberegn med de planer og regler, som gælder ved afgørelsen.
        // Grundlaget fra indsendelsen bliver ikke ændret.
        var preview = requests.Preview(request.UserId, input, request.Id);
        if (preview.Hours != request.Hours)
        {
            throw new HolidayRequestConflictException("Timeforbruget har ændret sig siden indsendelsen. Medarbejderen skal indsende en ny ansøgning med det aktuelle timetal.");
        }
        var balance = balances.Get(request.UserId, DateOnly.FromDateTime(DateTime.Today));
        var periods = balance.Periods.OrderBy(period => period.UsageEnd).ToArray();

        foreach (var day in preview.Days)
        {
            day.Id = Guid.NewGuid();
            day.RequestId = request.Id;
            day.Basis = "approval";
            db.Insert(day);
            ReserveDay(request, day, periods);
        }
        request.Hours = preview.Hours;
    }

    private void ReserveDay(HolidayRequest request, HolidayRequestDay day, HolidayBalancePeriod[] periods)
    {
        decimal remaining = day.Hours;
        foreach (var period in periods)
        {
            if (remaining == 0)
            {
                break;
            }
            if (period.Account != request.Account || day.Date < period.UsageStart || day.Date > period.UsageEnd || period.Hours <= 0)
            {
                continue;
            }
            var source = FindSource(request.UserId, period, day.WorkScheduleId);
            var usage = new HolidayUsage();
            usage.Id = Guid.NewGuid();
            usage.RequestDayId = day.Id;
            usage.UserId = request.UserId;
            usage.AccrualDayId = source.Id;
            usage.Date = day.Date;
            usage.Hours = Math.Min(remaining, period.Hours);
            db.Insert(usage);
            remaining -= usage.Hours;
            period.Hours -= usage.Hours;
        }
        if (remaining > 0)
        {
            throw new HolidayRequestConflictException("Der er ikke nok bogførte timer til perioden. Godkendelse med feriegæld eller forventet optjening er endnu ikke tilsluttet.");
        }
    }

    private HolidayAccrualDay FindSource(Guid userId, HolidayBalancePeriod period, Guid? targetScheduleId)
    {
        var monthIds = db.HolidayAccrualMonths.Where(month => month.UserId == userId).Select(month => month.Id).ToArray();
        var sources = db.HolidayAccrualDays.Where(day => monthIds.Contains(day.AccrualMonthId) &&
            day.Account == period.Account && day.EarningStart == period.EarningStart && day.EarningEnd == period.EarningEnd &&
            day.UsageStart == period.UsageStart && day.UsageEnd == period.UsageEnd).ToArray();
        decimal targetHours = WeeklyHours(targetScheduleId);
        foreach (var source in sources)
        {
            if (WeeklyHours(source.WorkScheduleId) != targetHours)
            {
                throw new HolidayRequestConflictException("Arbejdstiden har ændret sig. Saldoomregning skal tilsluttes, før ferien kan godkendes korrekt.");
            }
        }
        return sources[0];
    }

    private decimal WeeklyHours(Guid? scheduleId)
    {
        if (scheduleId == null)
        {
            throw new HolidayRequestConflictException("Der mangler en arbejdsplan til beregningen.");
        }
        var days = db.WorkScheduleDays.Where(day => day.WorkScheduleId == scheduleId && !day.IsDayOff).ToArray();
        decimal total = 0;
        foreach (var day in days)
        {
            if (day.StartTime == null || day.EndTime == null)
            {
                throw new HolidayRequestConflictException("Arbejdsplanen mangler et klokkeslæt.");
            }
            var duration = day.EndTime.Value - day.StartTime.Value;
            total += (decimal)duration.TotalHours;
        }
        return total;
    }
}

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
    private readonly HolidayDebtService debt;
    private readonly HolidayConversionService conversions;

    public HolidayDecisionService(MyDatabaseConnection database, HolidayRequestService requestService,
        HolidayDebtService debtService, HolidayConversionService conversionService)
    {
        db = database;
        requests = requestService;
        debt = debtService;
        conversions = conversionService;
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
        if (input.Approve)
        {
            HolidayLedgerService.LockRules(db);
        }
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
        conversions.Record(request.UserId);
        transaction.Commit();
        return request;
    }

    private void Approve(HolidayRequest request)
    {
        var input = new HolidayRequestInput();
        input.Account = request.Account;
        input.StartDate = request.StartDate;
        input.EndDate = request.EndDate;
        var preview = requests.Calculate(request.UserId, input, request.Id);
        if (preview.Hours != request.Hours)
        {
            throw new HolidayRequestConflictException("Timeforbruget har ændret sig. Medarbejderen skal indsende en ny ansøgning med det aktuelle timetal.");
        }
        foreach (var day in preview.Days)
        {
            day.Id = Guid.NewGuid();
            day.RequestId = request.Id;
            day.Basis = "approval";
            db.Insert(day);
        }
        // Hele forløbet valideres, før noget forbrug gemmes.
        var usage = debt.Allocate(request.UserId, request.Account, preview.Days);
        foreach (var item in usage)
        {
            db.Insert(item);
        }
        request.Hours = preview.Hours;
    }
}

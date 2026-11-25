using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayCancellationService
{
    private readonly MyDatabaseConnection db;

    public HolidayCancellationService(MyDatabaseConnection database)
    {
        db = database;
    }

    public HolidayRequest? Request(Guid userId, Guid requestId)
    {
        using var transaction = db.BeginTransaction();
        LockEmployee(userId);
        var request = db.HolidayRequests.SingleOrDefault(item => item.Id == requestId && item.UserId == userId);
        if (request == null) return null;
        if (request.CancellationStatus != "") return request;
        CheckFutureHoliday(request);
        request.CancellationStatus = "pending";
        request.CancellationRequestedAt = DateTime.UtcNow;
        db.Update(request);
        transaction.Commit();
        return request;
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
        if (request == null) return null;
        LockEmployee(request.UserId);
        request = db.HolidayRequests.Single(item => item.Id == requestId);
        string status = "declined";
        if (input.Approve) status = "approved";
        if (request.CancellationStatus != "pending")
        {
            if (request.CancellationStatus == status && request.CancellationDecidedBy == decidedBy &&
                request.CancellationComment == comment) return request;
            throw new HolidayRequestConflictException("Der er ingen afventende annulleringsanmodning.");
        }

        if (input.Approve)
        {
            CheckFutureHoliday(request);
            // Behold timer, perioder og dagværdier. Markér kun, at reservationen er frigivet.
            var dayIds = db.HolidayRequestDays.Where(day => day.RequestId == requestId).Select(day => day.Id).ToArray();
            db.HolidayUsage.Where(item => dayIds.Contains(item.RequestDayId) && item.CancelledAt == null)
                .Set(item => item.CancelledAt, DateTime.UtcNow).Update();
            request.Status = "cancelled";
        }
        request.CancellationStatus = status;
        request.CancellationDecidedAt = DateTime.UtcNow;
        request.CancellationDecidedBy = decidedBy;
        request.CancellationComment = comment;
        db.Update(request);
        transaction.Commit();
        return request;
    }

    private static void CheckFutureHoliday(HolidayRequest request)
    {
        if (request.Status != "approved")
        {
            throw new HolidayRequestConflictException("Kun godkendt ferie kan anmodes annulleret.");
        }
        if (request.StartDate <= DateOnly.FromDateTime(DateTime.Today))
        {
            throw new HolidayRequestConflictException("Ferien er allerede startet og kan ikke annulleres her.");
        }
    }

    private void LockEmployee(Guid userId)
    {
        // Samme lås som feriegodkendelse og bogføring.
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
    }
}

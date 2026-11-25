using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_requests")]
public class HolidayRequest
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("account")] public HolidayAccount Account { get; set; }
    [Column("start_date")] public DateOnly StartDate { get; set; }
    [Column("end_date")] public DateOnly EndDate { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("status")] public string Status { get; set; } = "pending";
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("withdrawn_at"), Nullable] public DateTime? WithdrawnAt { get; set; }
    [Column("decided_at"), Nullable] public DateTime? DecidedAt { get; set; }
    [Column("decided_by"), Nullable] public Guid? DecidedBy { get; set; }
    [Column("decision_comment")] public string DecisionComment { get; set; } = "";
    [Column("cancellation_status")] public string CancellationStatus { get; set; } = "";
    [Column("cancellation_requested_at"), Nullable] public DateTime? CancellationRequestedAt { get; set; }
    [Column("cancellation_decided_at"), Nullable] public DateTime? CancellationDecidedAt { get; set; }
    [Column("cancellation_decided_by"), Nullable] public Guid? CancellationDecidedBy { get; set; }
    [Column("cancellation_comment")] public string CancellationComment { get; set; } = "";
}

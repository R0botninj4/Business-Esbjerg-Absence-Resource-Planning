using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_obstacles")]
public class HolidayObstacle
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("operation_id")] public Guid OperationId { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("request_id"), Nullable] public Guid? RequestId { get; set; }
    [Column("kind")] public string Kind { get; set; } = "";
    [Column("start_date")] public DateOnly StartDate { get; set; }
    [Column("end_date")] public DateOnly EndDate { get; set; }
    [Column("documentation_reference")] public string DocumentationReference { get; set; } = "";
    [Column("status")] public string Status { get; set; } = "pending";
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("decided_at"), Nullable] public DateTime? DecidedAt { get; set; }
    [Column("decided_by"), Nullable] public Guid? DecidedBy { get; set; }
    [Column("comment")] public string Comment { get; set; } = "";
}

[Table("holiday_obstacle_days")]
public class HolidayObstacleDay
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("obstacle_id")] public Guid ObstacleId { get; set; }
    [Column("usage_id")] public Guid UsageId { get; set; }
    [Column("date")] public DateOnly Date { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("holiday_year_start")] public DateOnly HolidayYearStart { get; set; }
    [Column("waiting_days")] public int WaitingDays { get; set; }
    [Column("is_waiting_day")] public bool IsWaitingDay { get; set; }
    [Column("refunded_hours")] public decimal RefundedHours { get; set; }
}

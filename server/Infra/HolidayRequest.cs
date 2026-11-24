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
}

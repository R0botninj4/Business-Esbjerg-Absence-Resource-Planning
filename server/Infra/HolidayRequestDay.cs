using LinqToDB.Mapping;

namespace Infra;

[Table("holiday_request_days")]
public class HolidayRequestDay
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("request_id")] public Guid RequestId { get; set; }
    [Column("date")] public DateOnly Date { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("employment_id")] public Guid EmploymentId { get; set; }
    [Column("holiday_rule_id")] public Guid HolidayRuleId { get; set; }
    [Column("work_schedule_id"), Nullable] public Guid? WorkScheduleId { get; set; }
    [Column("calendar_day_id"), Nullable] public Guid? CalendarDayId { get; set; }
    [Column("description")] public string Description { get; set; } = "";
    // En fremtidig godkendelse kan gemme sit eget grundlag uden at overskrive indsendelsen.
    [Column("basis")] public string Basis { get; set; } = "submission";
}

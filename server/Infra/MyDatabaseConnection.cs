using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Infra;

public class MyDatabaseConnection : DataConnection
{
    public MyDatabaseConnection(DataOptions<MyDatabaseConnection> options)
        : base(options.Options)
    {
        // SQLite kan læse klokkeslæt som DateTime. Vi skal bruge TimeOnly.
        var mapping = new MappingSchema();
        mapping.SetConverter<DateTime, TimeOnly>(value => TimeOnly.FromDateTime(value));
        AddMappingSchema(mapping);
    }

    public ITable<User> Users => this.GetTable<User>();
    public ITable<Employment> Employments => this.GetTable<Employment>();
    public ITable<WorkSchedule> WorkSchedules => this.GetTable<WorkSchedule>();
    public ITable<WorkScheduleDay> WorkScheduleDays => this.GetTable<WorkScheduleDay>();
    public ITable<CalendarDay> CalendarDays => this.GetTable<CalendarDay>();
    public ITable<HolidayRule> HolidayRules => this.GetTable<HolidayRule>();
    public ITable<HolidayAccrualMonth> HolidayAccrualMonths => this.GetTable<HolidayAccrualMonth>();
    public ITable<HolidayAccrualDay> HolidayAccrualDays => this.GetTable<HolidayAccrualDay>();
    public ITable<HolidayRulePeriodSettings> HolidayRulePeriods => this.GetTable<HolidayRulePeriodSettings>();
}

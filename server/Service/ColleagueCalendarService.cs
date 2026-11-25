using Infra;
using LinqToDB;

namespace Service;

public class ColleagueCalendarService
{
    private readonly MyDatabaseConnection db;

    public ColleagueCalendarService(MyDatabaseConnection database)
    {
        db = database;
    }

    public ColleagueCalendarResult Get(DateOnly startDate, DateOnly endDate)
    {
        if (startDate == default || endDate == default || endDate < startDate)
        {
            throw new ArgumentException("Angiv en gyldig start- og slutdato.");
        }
        if (endDate.DayNumber - startDate.DayNumber > 365)
        {
            throw new ArgumentException("Hent højst 366 kalenderdage ad gangen.");
        }

        // Hent data én gang. Løkkerne nedenfor arbejder med almindelige arrays.
        var employments = db.Employments.Where(item => item.StartDate <= endDate &&
            (item.EndDate == null || item.EndDate >= startDate) && item.Type != EmploymentType.Hourly).ToArray();
        var employeeIds = employments.Select(item => item.UserId).ToArray();
        var employees = db.Users.Where(user => user.Role == "Employee" && employeeIds.Contains(user.Id))
            .OrderBy(user => user.Email).ToArray();
        var employmentIds = employments.Select(item => item.Id).ToArray();
        var schedules = db.WorkSchedules.Where(item => employmentIds.Contains(item.EmploymentId) &&
            item.StartDate <= endDate && (item.EndDate == null || item.EndDate >= startDate)).ToArray();
        var scheduleIds = schedules.Select(item => item.Id).ToArray();
        var workDays = db.WorkScheduleDays.Where(item => scheduleIds.Contains(item.WorkScheduleId)).ToArray();
        var calendarDays = db.CalendarDays.Where(item => item.Date >= startDate && item.Date <= endDate).ToArray();
        var holidays = db.HolidayRequests.Where(item => item.Status == "approved" &&
            employeeIds.Contains(item.UserId) && item.StartDate <= endDate && item.EndDate >= startDate).ToArray();
        var holidayIds = holidays.Select(item => item.Id).ToArray();
        var requestDays = db.HolidayRequestDays.Where(day => holidayIds.Contains(day.RequestId) && day.Basis == "approval").ToArray();
        var requestDayIds = requestDays.Select(day => day.Id).ToArray();
        var activeUsage = db.HolidayUsage.Where(item => requestDayIds.Contains(item.RequestDayId) &&
            item.CancelledAt == null && item.Date >= startDate && item.Date <= endDate).ToArray();
        // Del kun fravær med kolleger, aldrig sygdomstype eller dokumentation.
        var obstacles = db.HolidayObstacles.Where(item => employeeIds.Contains(item.UserId) && item.Status == "approved" &&
            item.StartDate <= endDate && item.EndDate >= startDate).ToArray();

        var result = new ColleagueCalendarResult();
        result.StartDate = startDate;
        result.EndDate = endDate;
        foreach (var employee in employees)
        {
            var colleague = new ColleagueCalendarEmployee();
            colleague.Id = employee.Id;
            colleague.Name = employee.Email;
            const string domain = "@businessesbjerg.com";
            if (colleague.Name.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
            {
                colleague.Name = colleague.Name[..^domain.Length];
            }
            result.Employees.Add(colleague);
        }
        foreach (var holiday in holidays)
        {
            var dayIds = requestDays.Where(day => day.RequestId == holiday.Id).Select(day => day.Id).ToArray();
            var dates = activeUsage.Where(item => dayIds.Contains(item.RequestDayId)).Select(item => item.Date).Distinct().OrderBy(date => date).ToList();
            if (dates.Count == 0) continue;
            var saved = new ColleagueCalendarHoliday();
            saved.Id = holiday.Id;
            saved.EmployeeId = holiday.UserId;
            saved.StartDate = holiday.StartDate;
            saved.EndDate = holiday.EndDate;
            saved.Dates = dates;
            result.Holidays.Add(saved);
        }

        var date = startDate;
        while (true)
        {
            var day = new ColleagueCalendarDay();
            day.Date = date;
            var calendarDay = calendarDays.Where(item => item.Date == date)
                .OrderByDescending(item => item.Version).FirstOrDefault();

            foreach (var employee in employees)
            {
                var cell = new ColleagueCalendarCell();
                cell.EmployeeId = employee.Id;
                cell.Status = "notEmployed";
                var employment = employments.SingleOrDefault(item => item.UserId == employee.Id &&
                    item.StartDate <= date && (item.EndDate == null || item.EndDate >= date));
                if (employment != null)
                {
                    cell.Status = "dayOff";
                    bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    bool companyClosed = calendarDay != null && !calendarDay.UsesHolidayHours;
                    if (!weekend && !companyClosed)
                    {
                        var schedule = schedules.SingleOrDefault(item => item.EmploymentId == employment.Id &&
                            item.StartDate <= date && (item.EndDate == null || item.EndDate >= date));
                        WorkScheduleDay? workDay = null;
                        if (schedule != null)
                        {
                            workDay = workDays.SingleOrDefault(item => item.WorkScheduleId == schedule.Id && item.Day == date.DayOfWeek);
                        }
                        if (workDay == null || (!workDay.IsDayOff &&
                            (workDay.StartTime == null || workDay.EndTime == null || workDay.EndTime <= workDay.StartTime)))
                        {
                            cell.Status = "missingSchedule";
                            day.MissingScheduleCount++;
                        }
                        else if (!workDay.IsDayOff)
                        {
                            cell.StartTime = workDay.StartTime;
                            cell.EndTime = workDay.EndTime;
                            day.ScheduledCount++;
                            cell.Status = "working";
                            bool onHoliday = result.Holidays.Any(item => item.EmployeeId == employee.Id && item.Dates.Contains(date));
                            if (onHoliday)
                            {
                                cell.Status = "holiday";
                                day.HolidayCount++;
                            }
                            else if (obstacles.Any(item => item.UserId == employee.Id && item.StartDate <= date && item.EndDate >= date))
                            {
                                cell.Status = "unavailable";
                                day.OtherAbsenceCount++;
                            }
                            else
                            {
                                day.PresentCount++;
                            }
                        }
                    }
                }
                day.Employees.Add(cell);
            }
            result.Days.Add(day);
            if (date == endDate) break;
            date = date.AddDays(1);
        }
        return result;
    }
}

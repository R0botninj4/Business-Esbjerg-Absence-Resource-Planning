using Infra;

namespace Service;

public class HolidayHoursService
{
    private readonly WorkScheduleService workSchedules;
    private readonly CalendarService calendar;

    public HolidayHoursService(WorkScheduleService schedules, CalendarService calendarService)
    {
        workSchedules = schedules;
        calendar = calendarService;
    }

    public HolidayHoursResult Calculate(Guid employmentId, DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("Slutdatoen må ikke være før startdatoen.");
        }

        var employment = workSchedules.FindEmployment(employmentId);
        if (employment == null)
        {
            throw new ArgumentException("Ansættelsen findes ikke.");
        }

        if (employment.Type == EmploymentType.Hourly)
        {
            throw new ArgumentException("Ferietimer beregnes kun for fuldtid og deltid.");
        }

        if (startDate < employment.StartDate)
        {
            throw new ArgumentException("Ferien må ikke starte før ansættelsen.");
        }

        if (employment.EndDate.HasValue && endDate > employment.EndDate.Value)
        {
            throw new ArgumentException("Ferien må ikke slutte efter ansættelsen.");
        }

        var result = new HolidayHoursResult();
        var date = startDate;

        while (true)
        {
            // Find planen igen for hver dato, fordi planen kan skifte under ferien.
            var calculatedDay = CalculateDate(employmentId, date);
            result.Days.Add(calculatedDay);
            result.Hours = result.Hours + calculatedDay.Hours;

            if (date == endDate)
            {
                break;
            }

            date = date.AddDays(1);
        }

        result.Hours = Math.Round(result.Hours, 2);
        return result;
    }

    private HolidayHoursDay CalculateDate(Guid employmentId, DateOnly date)
    {
        var result = new HolidayHoursDay();
        result.Date = date;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            result.Description = "Weekend";
            return result;
        }

        var schedule = workSchedules.GetForDate(employmentId, date);
        if (schedule == null)
        {
            throw new ArgumentException("Der mangler en ugeplan for " + date);
        }

        result.WorkScheduleId = schedule.Id;
        var days = workSchedules.GetDays(schedule.Id);
        WorkScheduleDay? workingDay = null;

        foreach (var day in days)
        {
            if (day.Day == date.DayOfWeek)
            {
                workingDay = day;
                break;
            }
        }

        if (workingDay == null)
        {
            throw new ArgumentException("Der mangler arbejdstider for " + date);
        }

        var calendarDay = calendar.GetForDate(date);
        if (calendarDay != null)
        {
            result.CalendarDayId = calendarDay.Id;

            if (!calendarDay.UsesHolidayHours)
            {
                result.Description = calendarDay.Name;
                return result;
            }
        }

        if (workingDay.IsDayOff)
        {
            result.Description = "Arbejdsfri dag";
            return result;
        }

        result.Description = "Arbejdsdag";
        result.Hours = HolidayHoursCalculator.CalculateDay(workingDay);
        return result;
    }
}

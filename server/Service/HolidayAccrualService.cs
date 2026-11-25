using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayAccrualService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayConversionService conversions;

    public HolidayAccrualService(
        MyDatabaseConnection database, HolidayConversionService conversionService)
    {
        db = database;
        conversions = conversionService;
    }

    public HolidayAccrualResult GetMonth(Guid userId, int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var posted = FindMonth(userId, first);
        if (posted != null)
        {
            return ReadPosted(posted);
        }

        var last = first.AddMonths(1).AddDays(-1);
        var forecast = Forecast(userId, last);
        var result = new HolidayAccrualResult();

        foreach (var day in forecast.Days)
        {
            if (day.Date >= first && day.Date <= last)
            {
                AddDay(result, day);
            }
        }

        return result;
    }

    public HolidayAccrualResult Forecast(Guid userId, DateOnly throughDate)
    {
        var employments = GetEmployments(userId);
        var firstEmployment = employments[0];
        if (throughDate < firstEmployment.StartDate)
        {
            throw new ArgumentException("Datoen ligger før ansættelsen.");
        }

        var result = new HolidayAccrualResult();
        result.IsPosted = true;
        var month = new DateOnly(firstEmployment.StartDate.Year, firstEmployment.StartDate.Month, 1);

        while (month <= throughDate)
        {
            AddForecastMonth(userId, month, throughDate, employments, result);

            if (month.Year == throughDate.Year && month.Month == throughDate.Month)
            {
                break;
            }

            month = month.AddMonths(1);
        }

        return result;
    }

    private void AddForecastMonth(Guid userId, DateOnly month, DateOnly throughDate,
        Employment[] employments, HolidayAccrualResult result)
    {
        var postedMonth = FindMonth(userId, month);
        if (postedMonth != null)
        {
            // Genbrug gemte tal, så historikken ikke bliver beregnet om.
            var savedDays = GetPostedDays(postedMonth.Id, throughDate);
            foreach (var day in savedDays)
            {
                AddDay(result, day);
            }

            return;
        }

        int previousCount = result.Days.Count;
        CalculateMonth(employments, month, throughDate, result);
        if (result.Days.Count > previousCount)
        {
            result.IsPosted = false;
        }
    }

    public HolidayAccrualResult Post(Guid userId, int year, int month, Guid postedBy)
    {
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (last >= today)
        {
            throw new ArgumentException("Kun afsluttede måneder kan bogføres.");
        }

        using var transaction = db.BeginTransaction();
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
        var existing = FindMonth(userId, first);
        if (existing != null)
        {
            return ReadPosted(existing);
        }

        var employments = GetEmployments(userId);
        ValidatePostingOrder(userId, first, last, employments);

        var result = GetMonth(userId, year, month);
        SaveMonth(userId, first, postedBy, result);
        conversions.Record(userId);
        transaction.Commit();

        result.IsPosted = true;
        return result;
    }

    public void PostMissingMonths(Guid userId)
    {
        var employments = GetEmployments(userId);
        var first = employments[0].StartDate;
        var month = new DateOnly(first.Year, first.Month, 1);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);
        var lastEmployment = employments[employments.Length - 1];

        while (month < currentMonth)
        {
            if (lastEmployment.EndDate.HasValue && month > lastEmployment.EndDate.Value)
            {
                break;
            }

            if (FindMonth(userId, month) == null)
            {
                // Tomt GUID betyder, at systemet har bogført automatisk.
                Post(userId, month.Year, month.Month, Guid.Empty);
            }

            month = month.AddMonths(1);
        }
    }

    private void ValidatePostingOrder(Guid userId, DateOnly first, DateOnly last, Employment[] employments)
    {
        var firstEmployment = employments[0];
        var lastEmployment = employments[employments.Length - 1];
        if (last < firstEmployment.StartDate ||
            (lastEmployment.EndDate.HasValue && first > lastEmployment.EndDate.Value))
        {
            throw new ArgumentException("Måneden ligger uden for ansættelsen.");
        }

        var firstEmploymentMonth = new DateOnly(firstEmployment.StartDate.Year, firstEmployment.StartDate.Month, 1);
        if (first > firstEmploymentMonth && FindMonth(userId, first.AddMonths(-1)) == null)
        {
            throw new ArgumentException("Bogfør den foregående måned først.");
        }
    }

    private void SaveMonth(Guid userId, DateOnly month, Guid postedBy, HolidayAccrualResult result)
    {
        // Post har startet en transaktion, så hele måneden bliver gemt samlet.
        var posting = new HolidayAccrualMonth();
        posting.Id = Guid.NewGuid();
        posting.UserId = userId;
        posting.Month = month;
        posting.PostedAt = DateTime.UtcNow;
        posting.PostedBy = postedBy;
        db.Insert(posting);

        foreach (var day in result.Days)
        {
            day.Id = Guid.NewGuid();
            day.AccrualMonthId = posting.Id;
            db.Insert(day);
        }
    }

    private void CalculateMonth(
        Employment[] employments, DateOnly month, DateOnly throughDate, HolidayAccrualResult result)
    {
        int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

        for (int dayNumber = 1; dayNumber <= daysInMonth; dayNumber++)
        {
            var date = new DateOnly(month.Year, month.Month, dayNumber);
            if (date > throughDate)
            {
                continue;
            }

            CalculateDate(employments, date, result);
        }
    }

    private void CalculateDate(Employment[] employments, DateOnly date, HolidayAccrualResult result)
    {
        var employment = FindEmployment(employments, date);
        if (employment == null)
        {
            return;
        }

        var rule = HolidayRuleService.FindForDate(db, date);
        if (rule == null || rule.Periods == null ||
            !rule.HolidayDaysPerYear.HasValue || !rule.SixthWeekHoursPerCalendarDay.HasValue)
        {
            throw new ArgumentException("Der mangler komplette optjeningsregler for " + date);
        }

        var schedule = db.WorkSchedules.SingleOrDefault(plan => plan.EmploymentId == employment.Id &&
            plan.StartDate <= date && (plan.EndDate == null || plan.EndDate >= date));
        if (schedule == null)
        {
            throw new ArgumentException("Der mangler en ugeplan for " + date);
        }

        decimal weeklyHours = GetWeeklyHours(schedule.Id);

        // Kontoen til uge 1–5 optjener dage, som omregnes til timer.
        var holidayPeriod = GetHolidayPeriod(rule.Periods, date);
        var holidayDay = CreateDay(date, employment, rule, schedule, holidayPeriod);
        holidayDay.Account = HolidayAccount.WeeksOneToFive;
        holidayDay.RawDays = GetHolidayDays(employments, date, rule, holidayPeriod, result);
        decimal hours = holidayDay.RawDays * weeklyHours;
        // Ugeplanen har fem hverdage, så omregningen fordeles over fem dage.
        holidayDay.RawHours = hours / 5;
        AddCalculatedDay(result, holidayDay);

        // Kontoen til 6. ferieuge optjener timer direkte.
        var sixthPeriod = GetSixthWeekPeriod(rule.Periods, date);
        var sixthDay = CreateDay(date, employment, rule, schedule, sixthPeriod);
        sixthDay.Account = HolidayAccount.SixthWeek;
        if (rule.SixthWeekEnabled)
        {
            decimal workTimeRatio = weeklyHours / rule.FullTimeHoursPerWeek;
            sixthDay.RawHours = GetSixthWeekHours(employments, date,
                rule.SixthWeekHoursPerMonth, rule.SixthWeekHoursPerCalendarDay.Value, workTimeRatio);
        }

        AddCalculatedDay(result, sixthDay);
    }

    private static HolidayAccountPeriod GetHolidayPeriod(HolidayRulePeriodSettings settings, DateOnly date)
    {
        int year = GetEarningYear(date, settings.HolidayEarningStartMonth, settings.HolidayEarningStartDay);
        var periods = HolidayRulePeriodsCalculator.Calculate(settings, year);
        return periods.WeeksOneToFive;
    }

    private static HolidayAccountPeriod GetSixthWeekPeriod(HolidayRulePeriodSettings settings, DateOnly date)
    {
        int year = GetEarningYear(date, settings.SixthWeekEarningStartMonth, settings.SixthWeekEarningStartDay);
        var periods = HolidayRulePeriodsCalculator.Calculate(settings, year);
        return periods.SixthWeek;
    }

    public decimal GetWeeklyHours(Guid scheduleId)
    {
        decimal hours = 0;
        var days = db.WorkScheduleDays.Where(day => day.WorkScheduleId == scheduleId).ToArray();
        if (days.Length != 5)
        {
            throw new ArgumentException("Ugeplanen skal have fem hverdage.");
        }

        foreach (var day in days)
        {
            if (day.IsDayOff)
            {
                continue;
            }

            if (!day.StartTime.HasValue || !day.EndTime.HasValue || day.EndTime <= day.StartTime)
            {
                throw new ArgumentException("Ugeplanen indeholder ugyldige arbejdstider.");
            }

            var duration = day.EndTime.Value - day.StartTime.Value;
            decimal dayHours = (decimal)duration.TotalHours;
            hours = hours + dayHours;
        }

        return hours;
    }

    private static decimal GetSixthWeekHours(Employment[] employments, DateOnly date,
        decimal monthlyRate, decimal dailyRate, decimal workTimeRatio)
    {
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        var first = new DateOnly(date.Year, date.Month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        bool fullMonth = CoversPeriod(employments, first, last);
        if (fullMonth)
        {
            decimal dailyHours = monthlyRate / daysInMonth;
            return dailyHours * workTimeRatio;
        }

        // Ved en delvis måned bruges satsen pr. ansat kalenderdag.
        int employedDays = CountEmployedDays(employments, first, date);
        decimal hoursBeforeToday = (employedDays - 1) * dailyRate;
        decimal hoursIncludingToday = employedDays * dailyRate;

        // Begræns beløbene til det, en hel måned ville give.
        if (hoursBeforeToday > monthlyRate)
        {
            hoursBeforeToday = monthlyRate;
        }

        if (hoursIncludingToday > monthlyRate)
        {
            hoursIncludingToday = monthlyRate;
        }

        decimal hours = hoursIncludingToday - hoursBeforeToday;
        return hours * workTimeRatio;
    }

    private static int CountEmployedDays(Employment[] employments, DateOnly first, DateOnly throughDate)
    {
        int count = 0;
        for (int day = 1; day <= throughDate.Day; day++)
        {
            var date = new DateOnly(first.Year, first.Month, day);
            if (FindEmployment(employments, date) != null)
            {
                count = count + 1;
            }
        }

        return count;
    }

    private static decimal GetHolidayDays(Employment[] employments, DateOnly date,
        HolidayRule rule, HolidayAccountPeriod period, HolidayAccrualResult result)
    {
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        decimal days = rule.HolidayDaysPerMonth / daysInMonth;
        bool fullYear = CoversPeriod(employments, period.EarningStart, period.EarningEnd);

        if (!fullYear || date != period.EarningEnd)
        {
            return days;
        }

        // Sidste dag suppleres til årstotalen. Allerede optjente dage trækkes ikke tilbage.
        decimal previousDays = GetPreviousHolidayDays(result, period.EarningStart);
        decimal annualDays = rule.HolidayDaysPerYear.GetValueOrDefault();
        decimal remainingDays = annualDays - previousDays;
        if (remainingDays > days)
        {
            days = remainingDays;
        }

        return days;
    }

    private static decimal GetPreviousHolidayDays(HolidayAccrualResult result, DateOnly earningStart)
    {
        decimal days = 0;
        foreach (var previous in result.Days)
        {
            if (previous.Account == HolidayAccount.WeeksOneToFive && previous.EarningStart == earningStart)
            {
                days = days + previous.RawDays;
            }
        }

        return days;
    }

    private static int GetEarningYear(DateOnly date, int month, int day)
    {
        int year = date.Year;
        if (date < new DateOnly(year, month, day))
        {
            year = year - 1;
        }

        return year;
    }

    private static HolidayAccrualDay CreateDay(DateOnly date, Employment employment,
        HolidayRule rule, WorkSchedule schedule, HolidayAccountPeriod period)
    {
        var day = new HolidayAccrualDay();
        day.Date = date;
        day.EmploymentId = employment.Id;
        day.HolidayRuleId = rule.Id;
        day.WorkScheduleId = schedule.Id;
        day.EarningStart = period.EarningStart;
        day.EarningEnd = period.EarningEnd;
        day.UsageStart = period.UsageStart;
        day.UsageEnd = period.UsageEnd;
        return day;
    }

    private static void AddCalculatedDay(HolidayAccrualResult result, HolidayAccrualDay day)
    {
        decimal previousHours = GetPreviousRawHours(result, day.Account, day.EarningStart);
        decimal totalHours = previousHours + day.RawHours;

        // Afrund summen indtil i dag og træk den tidligere afrundede sum fra.
        // Så bliver små decimalrester ført videre i stedet for at gå tabt.
        decimal previousRoundedHours = Math.Round(previousHours, 2);
        decimal totalRoundedHours = Math.Round(totalHours, 2);
        day.Hours = totalRoundedHours - previousRoundedHours;
        AddDay(result, day);
    }

    private static decimal GetPreviousRawHours(
        HolidayAccrualResult result, HolidayAccount account, DateOnly earningStart)
    {
        decimal hours = 0;
        foreach (var previous in result.Days)
        {
            if (previous.Account == account && previous.EarningStart == earningStart)
            {
                hours = hours + previous.RawHours;
            }
        }

        return hours;
    }

    private static void AddDay(HolidayAccrualResult result, HolidayAccrualDay day)
    {
        result.Days.Add(day);
        if (day.Account == HolidayAccount.WeeksOneToFive)
        {
            result.WeeksOneToFiveHours = result.WeeksOneToFiveHours + day.Hours;
        }
        else
        {
            result.SixthWeekHours = result.SixthWeekHours + day.Hours;
        }
    }

    private HolidayAccrualMonth? FindMonth(Guid userId, DateOnly month)
    {
        return db.HolidayAccrualMonths.SingleOrDefault(posting =>
            posting.UserId == userId && posting.Month == month);
    }

    private Employment[] GetEmployments(Guid userId)
    {
        var employments = db.Employments
            .Where(employment => employment.UserId == userId && employment.Type != EmploymentType.Hourly)
            .OrderBy(employment => employment.StartDate).ToArray();
        if (employments.Length == 0)
        {
            throw new ArgumentException("Medarbejderen har ingen fuldtids- eller deltidsansættelse.");
        }

        return employments;
    }

    private static Employment? FindEmployment(Employment[] employments, DateOnly date)
    {
        Employment? found = null;
        foreach (var employment in employments)
        {
            if (employment.StartDate > date)
            {
                continue;
            }

            if (employment.EndDate.HasValue && employment.EndDate.Value < date)
            {
                continue;
            }

            if (found != null)
            {
                throw new ArgumentException("Medarbejderens ansættelsesperioder overlapper.");
            }

            found = employment;
        }

        return found;
    }

    private static bool CoversPeriod(Employment[] employments, DateOnly start, DateOnly end)
    {
        var nextDate = start;
        foreach (var employment in employments)
        {
            if (employment.EndDate.HasValue && employment.EndDate.Value < nextDate)
            {
                continue;
            }

            if (employment.StartDate > nextDate)
            {
                return false;
            }

            if (!employment.EndDate.HasValue || employment.EndDate.Value >= end)
            {
                return true;
            }

            nextDate = employment.EndDate.Value.AddDays(1);
        }

        return false;
    }

    private HolidayAccrualResult ReadPosted(HolidayAccrualMonth posting)
    {
        var result = new HolidayAccrualResult();
        result.IsPosted = true;
        var days = GetPostedDays(posting.Id, DateOnly.MaxValue);
        foreach (var day in days)
        {
            AddDay(result, day);
        }

        return result;
    }

    private HolidayAccrualDay[] GetPostedDays(Guid monthId, DateOnly throughDate)
    {
        return db.HolidayAccrualDays
            .Where(day => day.AccrualMonthId == monthId && day.Date <= throughDate)
            .OrderBy(day => day.Date)
            .ThenBy(day => day.Account)
            .ToArray();
    }
}

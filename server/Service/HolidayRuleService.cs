using Infra;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;

namespace Service;

public class HolidayRuleService
{
    private readonly MyDatabaseConnection db;
    private readonly IConfiguration configuration;
    private readonly HolidayDebtService debt;

    public HolidayRuleService(MyDatabaseConnection database, IConfiguration settings, HolidayDebtService debtService)
    {
        db = database;
        configuration = settings;
        debt = debtService;
    }

    public HolidayRuleSettings[] GetPresets()
    {
        var presets = configuration.GetSection("HolidayRulePresets")
            .Get<HolidayRuleSettings[]>();

        if (presets == null)
        {
            return new HolidayRuleSettings[0];
        }

        return presets;
    }

    public HolidayRule[] GetHistory()
    {
        return ReadHistory(db);
    }

    public static HolidayRule[] ReadHistory(MyDatabaseConnection database)
    {
        var rules = database.HolidayRules
            .OrderByDescending(rule => rule.EffectiveFrom)
            .ThenByDescending(rule => rule.Version)
            .ToArray();

        foreach (var rule in rules)
        {
            rule.Periods = database.HolidayRulePeriods.SingleOrDefault(period => period.HolidayRuleId == rule.Id);
        }

        return rules;
    }

    public HolidayRule? Find(Guid id)
    {
        var rule = db.HolidayRules.SingleOrDefault(rule => rule.Id == id);
        if (rule != null)
        {
            rule.Periods = GetPeriodSettings(rule.Id);
        }

        return rule;
    }

    public HolidayRule? GetForDate(DateOnly date)
    {
        return FindForDate(db, date);
    }

    public static HolidayRule? FindForDate(MyDatabaseConnection database, DateOnly date)
    {
        var rule = database.HolidayRules
            .Where(rule => rule.EffectiveFrom <= date)
            .OrderByDescending(rule => rule.EffectiveFrom)
            .ThenByDescending(rule => rule.Version)
            .FirstOrDefault();

        if (rule != null)
        {
            rule.Periods = database.HolidayRulePeriods.SingleOrDefault(period => period.HolidayRuleId == rule.Id);
        }

        return rule;
    }

    private HolidayRulePeriodSettings? GetPeriodSettings(Guid ruleId)
    {
        return db.HolidayRulePeriods.SingleOrDefault(periods => periods.HolidayRuleId == ruleId);
    }

    public HolidayRule Create(
        DateOnly effectiveFrom, HolidayRuleSettings settings, Guid createdBy)
    {
        Validate(settings);
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (effectiveFrom == default)
        {
            throw new ArgumentException("Angiv en ikrafttrædelsesdato.");
        }

        using var transaction = db.BeginTransaction();
        // Samme lås som godkendelse. Regler kan ikke ændres midt i en afgørelse.
        HolidayLedgerService.LockRules(db);

        // Første opsætning kan beskrive eksisterende ansættelser.
        // Senere ændringer må ikke ændre fortidens regler.
        if (effectiveFrom <= today && db.HolidayRules.Any())
        {
            throw new ArgumentException("Ændringer skal starte på en fremtidig dato.");
        }

        var previous = db.HolidayRules
            .Where(rule => rule.EffectiveFrom == effectiveFrom)
            .OrderByDescending(rule => rule.Version)
            .FirstOrDefault();

        int version = 1;
        if (previous != null)
        {
            version = previous.Version + 1;
        }

        var rule = new HolidayRule();
        rule.Id = Guid.NewGuid();
        rule.Name = settings.Name.Trim();
        rule.EffectiveFrom = effectiveFrom;
        rule.Version = version;
        rule.HolidayDaysPerMonth = settings.HolidayDaysPerMonth;
        rule.HolidayDaysPerYear = settings.HolidayDaysPerYear;
        rule.SixthWeekHoursPerCalendarDay = settings.SixthWeekHoursPerCalendarDay;
        rule.SixthWeekEnabled = settings.SixthWeekEnabled;
        rule.SixthWeekHoursPerMonth = settings.SixthWeekHoursPerMonth;
        rule.FullTimeHoursPerWeek = settings.FullTimeHoursPerWeek;
        rule.MaximumDebtDays = settings.MaximumDebtDays;
        rule.AllowTransfer = settings.AllowTransfer;
        rule.MaximumTransferDays = settings.MaximumTransferDays;
        rule.CreatedAt = DateTime.UtcNow;
        rule.CreatedBy = createdBy;

        var periods = new HolidayRulePeriodSettings();
        periods.HolidayRuleId = rule.Id;
        periods.HolidayEarningStartMonth = settings.HolidayEarningStartMonth;
        periods.HolidayEarningStartDay = settings.HolidayEarningStartDay;
        periods.HolidayUsageStartMonth = settings.HolidayUsageStartMonth;
        periods.HolidayUsageStartDay = settings.HolidayUsageStartDay;
        periods.HolidayUsageStartYearOffset = settings.HolidayUsageStartYearOffset;
        periods.HolidayUsageEndMonth = settings.HolidayUsageEndMonth;
        periods.HolidayUsageEndDay = settings.HolidayUsageEndDay;
        periods.HolidayUsageEndYearOffset = settings.HolidayUsageEndYearOffset;
        periods.SixthWeekEarningStartMonth = settings.SixthWeekEarningStartMonth;
        periods.SixthWeekEarningStartDay = settings.SixthWeekEarningStartDay;
        periods.SixthWeekUsageStartMonth = settings.SixthWeekUsageStartMonth;
        periods.SixthWeekUsageStartDay = settings.SixthWeekUsageStartDay;
        periods.SixthWeekUsageStartYearOffset = settings.SixthWeekUsageStartYearOffset;
        periods.SixthWeekUsageEndMonth = settings.SixthWeekUsageEndMonth;
        periods.SixthWeekUsageEndDay = settings.SixthWeekUsageEndDay;
        periods.SixthWeekUsageEndYearOffset = settings.SixthWeekUsageEndYearOffset;
        periods.MainHolidayDays = settings.MainHolidayDays;
        periods.MainHolidayStartMonth = settings.MainHolidayStartMonth;
        periods.MainHolidayStartDay = settings.MainHolidayStartDay;
        periods.MainHolidayEndMonth = settings.MainHolidayEndMonth;
        periods.MainHolidayEndDay = settings.MainHolidayEndDay;
        periods.MainHolidayNoticeMonths = settings.MainHolidayNoticeMonths;
        periods.OtherHolidayNoticeMonths = settings.OtherHolidayNoticeMonths;
        periods.DebtDeadlineMonth = settings.DebtDeadlineMonth;
        periods.DebtDeadlineDay = settings.DebtDeadlineDay;
        periods.HolidayTransferDeadlineMonth = settings.HolidayTransferDeadlineMonth;
        periods.HolidayTransferDeadlineDay = settings.HolidayTransferDeadlineDay;
        periods.SixthWeekTransferDeadlineMonth = settings.SixthWeekTransferDeadlineMonth;
        periods.SixthWeekTransferDeadlineDay = settings.SixthWeekTransferDeadlineDay;

        // Kontrollér periodernes datoer, inden noget bliver gemt.
        HolidayRulePeriodsCalculator.Calculate(periods, effectiveFrom.Year);
        rule.Periods = periods;

        // Gem en ny version i stedet for at overskrive den gamle.
        db.Insert(rule);
        db.Insert(periods);
        var userIds = db.HolidayUsage.Select(item => item.UserId).Distinct().OrderBy(id => id).ToArray();
        foreach (var userId in userIds)
        {
            db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
            debt.ValidateExisting(userId);
        }
        transaction.Commit();
        return rule;
    }

    private static void Validate(HolidayRuleSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Name) || settings.Name.Length > 100)
        {
            throw new ArgumentException("Angiv et navn på højst 100 tegn.");
        }

        if (settings.HolidayDaysPerMonth < 0 || settings.SixthWeekHoursPerMonth < 0)
        {
            throw new ArgumentException("Optjeningssatser må ikke være negative.");
        }

        if (settings.HolidayDaysPerYear < settings.HolidayDaysPerMonth * 11)
        {
            throw new ArgumentException("Årstotalen må ikke være mindre end 11 måneders optjening.");
        }

        if (settings.SixthWeekHoursPerCalendarDay < 0)
        {
            throw new ArgumentException("Den daglige optjeningssats må ikke være negativ.");
        }

        if (settings.FullTimeHoursPerWeek <= 0)
        {
            throw new ArgumentException("Fuldtidstimer pr. uge skal være større end 0.");
        }

        if (settings.MaximumDebtDays < 0 || settings.MaximumTransferDays < 0)
        {
            throw new ArgumentException("Grænser for gæld og overførsel må ikke være negative.");
        }

        if (!settings.SixthWeekEnabled &&
            (settings.SixthWeekHoursPerMonth != 0 || settings.SixthWeekHoursPerCalendarDay != 0))
        {
            throw new ArgumentException("Optjeningen skal være 0, når 6. ferieuge er slået fra.");
        }
    }
}

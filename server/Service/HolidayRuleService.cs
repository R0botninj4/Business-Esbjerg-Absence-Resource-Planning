using Infra;
using LinqToDB;
using Microsoft.Extensions.Configuration;

namespace Service;

public class HolidayRuleService
{
    private readonly MyDatabaseConnection db;
    private readonly IConfiguration configuration;

    public HolidayRuleService(MyDatabaseConnection database, IConfiguration settings)
    {
        db = database;
        configuration = settings;
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
        return db.HolidayRules
            .OrderByDescending(rule => rule.EffectiveFrom)
            .ThenByDescending(rule => rule.Version)
            .ToArray();
    }

    public HolidayRule? Find(Guid id)
    {
        return db.HolidayRules.SingleOrDefault(rule => rule.Id == id);
    }

    public HolidayRule? GetForDate(DateOnly date)
    {
        return db.HolidayRules
            .Where(rule => rule.EffectiveFrom <= date)
            .OrderByDescending(rule => rule.EffectiveFrom)
            .ThenByDescending(rule => rule.Version)
            .FirstOrDefault();
    }

    public HolidayRule Create(
        DateOnly effectiveFrom, HolidayRuleSettings settings, Guid createdBy)
    {
        Validate(settings);
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (effectiveFrom < today)
        {
            throw new ArgumentException("Nye regler må ikke starte i fortiden.");
        }

        using var transaction = db.BeginTransaction();

        // Første opsætning må starte i dag. Senere ændringer gælder fremadrettet.
        if (effectiveFrom == today && db.HolidayRules.Any())
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
        rule.SixthWeekEnabled = settings.SixthWeekEnabled;
        rule.SixthWeekHoursPerMonth = settings.SixthWeekHoursPerMonth;
        rule.FullTimeHoursPerWeek = settings.FullTimeHoursPerWeek;
        rule.MaximumDebtDays = settings.MaximumDebtDays;
        rule.AllowTransfer = settings.AllowTransfer;
        rule.MaximumTransferDays = settings.MaximumTransferDays;
        rule.CreatedAt = DateTime.UtcNow;
        rule.CreatedBy = createdBy;

        // Gem en ny version i stedet for at overskrive den gamle.
        db.Insert(rule);
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

        if (settings.FullTimeHoursPerWeek <= 0)
        {
            throw new ArgumentException("Fuldtidstimer pr. uge skal være større end 0.");
        }

        if (settings.MaximumDebtDays < 0 || settings.MaximumTransferDays < 0)
        {
            throw new ArgumentException("Grænser for gæld og overførsel må ikke være negative.");
        }

        if (!settings.SixthWeekEnabled && settings.SixthWeekHoursPerMonth != 0)
        {
            throw new ArgumentException("Optjeningen skal være 0, når 6. ferieuge er slået fra.");
        }
    }
}

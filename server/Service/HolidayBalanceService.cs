using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

public class HolidayBalanceService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayAccrualService accrual;
    private readonly HolidayLedgerService ledger;
    private readonly HolidayConversionService conversions;

    public HolidayBalanceService(MyDatabaseConnection database, HolidayAccrualService accrualService,
        HolidayLedgerService ledgerService, HolidayConversionService conversionService)
    {
        db = database;
        accrual = accrualService;
        ledger = ledgerService;
        conversions = conversionService;
    }

    public HolidayBalanceResult Get(Guid userId, DateOnly date)
    {
        var credits = ledger.PostedCredits(userId, date);
        return ledger.Calculate(userId, date, credits, false);
    }

    public HolidayBalanceResult Forecast(Guid userId, DateOnly date)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Vælg i dag eller en fremtidig dato.");
        }
        if (date.Year > 9997)
        {
            throw new ArgumentException("Datoens år er for stort.");
        }
        // Forecast læser kun. Den bogfører hverken optjening, forbrug eller omregninger.
        var credits = accrual.Forecast(userId, date).Days.ToArray();
        return ledger.Calculate(userId, date, credits, true);
    }

    public HolidayBalanceResult Reconcile(Guid userId)
    {
        // Bogfør manglende måneder før saldoen vises. Hver måned gemmes kun én gang.
        // Dette skal ske før transaktionen nedenfor, fordi bogføring har sin egen transaktion.
        accrual.PostMissingMonths(userId);

        using var transaction = db.BeginTransaction();
        db.Query<Guid>("SELECT id FROM users WHERE id=@id FOR UPDATE", new DataParameter("id", userId)).ToArray();
        conversions.Record(userId);
        var result = Get(userId, DateOnly.FromDateTime(DateTime.Today));
        transaction.Commit();
        return result;
    }

    public HolidayBalanceConversion[] GetConversions(Guid userId)
    {
        return db.HolidayBalanceConversions.Where(item => item.UserId == userId)
            .OrderBy(item => item.Date).ThenBy(item => item.CreatedAt).ToArray();
    }
}

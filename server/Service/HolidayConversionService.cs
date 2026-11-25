using Infra;
using LinqToDB;

namespace Service;

public class HolidayConversionService
{
    private readonly MyDatabaseConnection db;
    private readonly HolidayLedgerService ledger;

    public HolidayConversionService(MyDatabaseConnection database, HolidayLedgerService ledgerService)
    {
        db = database;
        ledger = ledgerService;
    }

    // Bruges inde i den skrivende handlings transaktion og medarbejderlås.
    public void Record(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var plans = ledger.Plans(userId).Where(plan => plan.StartDate <= today).ToArray();
        var saved = db.HolidayBalanceConversions.Where(item => item.UserId == userId).ToArray();
        var usage = db.HolidayUsage.Where(item => item.UserId == userId).ToArray();
        for (int index = 1; index < plans.Length; index++)
        {
            var previous = plans[index - 1];
            var plan = plans[index];
            decimal oldLength = ledger.DayLength(previous.Id);
            decimal newLength = ledger.DayLength(plan.Id);
            if (oldLength == newLength)
            {
                continue;
            }
            var date = plan.StartDate.AddDays(-1);
            var credits = ledger.PostedCredits(userId, date);
            var knownUsage = KnownUsage(usage, plan.StartDate);
            var balance = ledger.Calculate(userId, date, credits, false, knownUsage);
            var adjustmentIds = db.HolidayBalanceAdjustments.Where(item => item.UserId == userId && item.Date <= date)
                .Select(item => item.Id).ToArray();
            // Poster er uforanderlige. Nye poster giver et nyt grundlag.
            string basisKey = credits.Length + "/" + knownUsage.Length + "/" + adjustmentIds.Length;
            foreach (var period in balance.Periods)
            {
                if (period.UsageEnd < plan.StartDate)
                {
                    continue;
                }
                var earlier = saved.Where(item => item.WorkScheduleId == plan.Id && item.Account == period.Account &&
                    item.EarningStart == period.EarningStart && item.EarningEnd == period.EarningEnd &&
                    item.UsageStart == period.UsageStart && item.UsageEnd == period.UsageEnd).ToArray();
                if (earlier.Any(item => item.BasisKey == basisKey))
                {
                    continue;
                }
                decimal days = period.Days - period.ReservedDays;
                decimal before = Math.Round(days * oldLength, 2);
                decimal after = Math.Round(days * newLength, 2);
                decimal change = after - before - earlier.Sum(item => item.Hours);
                if (change == 0)
                {
                    continue;
                }
                var record = new HolidayBalanceConversion();
                record.Id = Guid.NewGuid();
                record.UserId = userId;
                record.PreviousScheduleId = previous.Id;
                record.WorkScheduleId = plan.Id;
                record.Account = period.Account;
                record.EarningStart = period.EarningStart;
                record.EarningEnd = period.EarningEnd;
                record.UsageStart = period.UsageStart;
                record.UsageEnd = period.UsageEnd;
                record.Date = plan.StartDate;
                record.RemainingDays = days;
                record.BeforeHours = before;
                record.AfterHours = after;
                // Senere bogføring supplerer omregningen uden at overskrive gamle poster.
                record.Hours = change;
                record.BasisKey = basisKey;
                record.CreatedAt = DateTime.UtcNow;
                db.Insert(record);
            }
        }
    }

    private HolidayUsage[] KnownUsage(HolidayUsage[] usage, DateOnly date)
    {
        var result = new List<HolidayUsage>();
        foreach (var item in usage)
        {
            // En senere annullering må ikke ændre grundlaget for en gammel omregning.
            if (item.CancelledAt.HasValue && DateOnly.FromDateTime(item.CancelledAt.Value.ToLocalTime()) < date)
            {
                continue;
            }
            var day = db.HolidayRequestDays.Single(day => day.Id == item.RequestDayId);
            var request = db.HolidayRequests.Single(request => request.Id == day.RequestId);
            if (request.DecidedAt.HasValue && DateOnly.FromDateTime(request.DecidedAt.Value.ToLocalTime()) < date)
            {
                result.Add(item);
            }
        }
        return result.ToArray();
    }
}

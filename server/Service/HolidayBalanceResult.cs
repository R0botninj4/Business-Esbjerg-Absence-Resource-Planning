using Infra;

namespace Service;

public class HolidayBalanceResult
{
    public DateOnly Date { get; set; }

    // Bogført optjening og rettelser inden for en gyldig afholdelsesperiode.
    public decimal WeeksOneToFiveHours { get; set; }

    public decimal SixthWeekHours { get; set; }

    // Forbrug og saldoomregning er endnu ikke koblet på denne oversigt.
    public bool IncludesHolidayUsage { get; set; }

    public bool IncludesScheduleConversions { get; set; }

    public List<HolidayBalancePeriod> Periods { get; set; } = new List<HolidayBalancePeriod>();
}

public class HolidayBalancePeriod
{
    public HolidayAccount Account { get; set; }

    public DateOnly EarningStart { get; set; }

    public DateOnly EarningEnd { get; set; }

    public DateOnly UsageStart { get; set; }

    public DateOnly UsageEnd { get; set; }

    public decimal PostedHours { get; set; }

    public decimal AdjustmentHours { get; set; }

    public decimal Hours { get; set; }

    // To poster må kun lægges sammen, hvis konto og begge perioder er ens.
    public bool Matches(HolidayAccrualDay day)
    {
        if (Account != day.Account)
        {
            return false;
        }

        if (EarningStart != day.EarningStart)
        {
            return false;
        }

        if (EarningEnd != day.EarningEnd)
        {
            return false;
        }

        if (UsageStart != day.UsageStart)
        {
            return false;
        }

        if (UsageEnd != day.UsageEnd)
        {
            return false;
        }

        return true;
    }
}

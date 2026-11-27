using System.Globalization;

namespace CoreBanking.Ledger.Domain.Periods;

public readonly record struct AccountingPeriod
{
    public int Year { get; }

    public int Month { get; }

    private AccountingPeriod(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public DateOnly StartDate => new(Year, Month, 1);

    public DateOnly EndDate => StartDate.AddMonths(1).AddDays(-1);

    public static AccountingPeriod Of(DateOnly date) => new(date.Year, date.Month);

    public static AccountingPeriod Parse(string text)
    {
        if (!DateOnly.TryParseExact(text, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var firstDay))
        {
            throw new DomainException("period.invalid_format", "Period must use the yyyy-MM format.");
        }

        return Of(firstDay);
    }

    public AccountingPeriod Next() => Month == 12 ? new(Year + 1, 1) : new(Year, Month + 1);

    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}

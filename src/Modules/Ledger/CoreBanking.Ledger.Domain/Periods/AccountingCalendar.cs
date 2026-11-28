namespace CoreBanking.Ledger.Domain.Periods;

public sealed class AccountingCalendar
{
    // The ledger has a single calendar, stored as one row.
    public int Id { get; private set; } = 1;

    public AccountingPeriod? LastClosed { get; private set; }

    public void EnsureCanBook(DateOnly bookingDate, DateOnly today)
    {
        if (bookingDate > today)
        {
            throw new DomainException("entry.booking_date_in_future", "Booking date cannot be in the future.");
        }

        if (LastClosed is { } closed && bookingDate <= closed.EndDate)
        {
            throw new DomainException("entry.booking_date_in_closed_period", $"Period {closed} is closed.");
        }
    }

    public void Close(AccountingPeriod period, DateOnly today)
    {
        if (IsClosed(period))
        {
            throw new DomainException("period.already_closed", $"Period {period} is already closed.");
        }

        if (LastClosed is { } last && period != last.Next())
        {
            throw new DomainException("period.out_of_order", $"The next period to close is {last.Next()}.");
        }

        if (period.EndDate >= today)
        {
            throw new DomainException("period.not_ended", $"Period {period} has not ended yet.");
        }

        LastClosed = period;
    }

    public bool IsClosed(AccountingPeriod period) => LastClosed is { } last && period.EndDate <= last.EndDate;
}

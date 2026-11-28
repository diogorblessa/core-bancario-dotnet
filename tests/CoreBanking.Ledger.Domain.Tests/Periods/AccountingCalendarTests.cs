using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Domain.Tests.Periods;

public class AccountingCalendarTests
{
    private static readonly DateOnly Today = new(2026, 11, 24);

    private static AccountingCalendar CalendarClosedThrough(string period)
    {
        var calendar = new AccountingCalendar();
        calendar.Close(AccountingPeriod.Parse(period), Today);
        return calendar;
    }

    [Fact]
    public void EnsureCanBook_FutureDate_Throws()
    {
        var calendar = new AccountingCalendar();

        var ex = Assert.Throws<DomainException>(() => calendar.EnsureCanBook(new DateOnly(2026, 11, 25), Today));

        Assert.Equal("entry.booking_date_in_future", ex.Code);
    }

    [Fact]
    public void EnsureCanBook_DateInClosedPeriod_Throws()
    {
        var calendar = CalendarClosedThrough("2026-10");

        var ex = Assert.Throws<DomainException>(() => calendar.EnsureCanBook(new DateOnly(2026, 10, 31), Today));

        Assert.Equal("entry.booking_date_in_closed_period", ex.Code);
    }

    [Fact]
    public void EnsureCanBook_FirstDayAfterClosedPeriod_Succeeds()
    {
        var calendar = CalendarClosedThrough("2026-10");

        var ex = Record.Exception(() => calendar.EnsureCanBook(new DateOnly(2026, 11, 1), Today));

        Assert.Null(ex);
    }

    [Fact]
    public void Close_FirstPeriod_SetsLastClosed()
    {
        var calendar = new AccountingCalendar();

        calendar.Close(AccountingPeriod.Parse("2026-09"), Today);

        Assert.Equal(AccountingPeriod.Parse("2026-09"), calendar.LastClosed);
    }

    [Fact]
    public void Close_NextPeriod_Succeeds()
    {
        var calendar = CalendarClosedThrough("2026-09");

        calendar.Close(AccountingPeriod.Parse("2026-10"), Today);

        Assert.Equal(AccountingPeriod.Parse("2026-10"), calendar.LastClosed);
    }

    [Fact]
    public void Close_OutOfOrder_Throws()
    {
        var calendar = CalendarClosedThrough("2026-09");

        var ex = Assert.Throws<DomainException>(() => calendar.Close(AccountingPeriod.Parse("2026-11"), Today));

        Assert.Equal("period.out_of_order", ex.Code);
    }

    [Fact]
    public void Close_CurrentMonth_Throws()
    {
        var calendar = CalendarClosedThrough("2026-10");

        var ex = Assert.Throws<DomainException>(() => calendar.Close(AccountingPeriod.Parse("2026-11"), Today));

        Assert.Equal("period.not_ended", ex.Code);
    }

    [Fact]
    public void Close_Twice_Throws()
    {
        var calendar = CalendarClosedThrough("2026-10");

        var ex = Assert.Throws<DomainException>(() => calendar.Close(AccountingPeriod.Parse("2026-10"), Today));

        Assert.Equal("period.already_closed", ex.Code);
    }

    [Fact]
    public void IsClosed_IncludesEarlierPeriods()
    {
        var calendar = CalendarClosedThrough("2026-10");

        Assert.True(calendar.IsClosed(AccountingPeriod.Parse("2026-10")));
        Assert.True(calendar.IsClosed(AccountingPeriod.Parse("2026-03")));
        Assert.False(calendar.IsClosed(AccountingPeriod.Parse("2026-11")));
    }
}

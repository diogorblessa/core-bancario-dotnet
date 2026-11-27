using Microsoft.Extensions.Time.Testing;

namespace CoreBanking.Ledger.Domain.Tests;

public class LedgerClockTests
{
    [Fact]
    public void Today_At0130Utc_IsPreviousDayInSaoPaulo()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 1, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 24), LedgerClock.Today(time));
    }

    [Fact]
    public void Today_At0300Utc_IsSameDayInSaoPaulo()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 3, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 25), LedgerClock.Today(time));
    }
}

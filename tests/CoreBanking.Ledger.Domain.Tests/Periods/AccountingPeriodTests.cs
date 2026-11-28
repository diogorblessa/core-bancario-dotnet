using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Domain.Tests.Periods;

public class AccountingPeriodTests
{
    [Fact]
    public void Parse_ValidText_RoundTrips()
    {
        var period = AccountingPeriod.Parse("2026-10");

        Assert.Equal(2026, period.Year);
        Assert.Equal(10, period.Month);
        Assert.Equal("2026-10", period.ToString());
    }

    [Theory]
    [InlineData("2026/10")]
    [InlineData("2026-13")]
    [InlineData("2026-1")]
    [InlineData("")]
    public void Parse_InvalidFormat_Throws(string text)
    {
        var ex = Assert.Throws<DomainException>(() => AccountingPeriod.Parse(text));

        Assert.Equal("period.invalid_format", ex.Code);
    }

    [Fact]
    public void Of_ReturnsMonthOfDate()
    {
        var period = AccountingPeriod.Of(new DateOnly(2026, 11, 24));

        Assert.Equal(AccountingPeriod.Parse("2026-11"), period);
    }

    [Fact]
    public void Next_InDecember_RollsOverToJanuary()
    {
        var next = AccountingPeriod.Parse("2026-12").Next();

        Assert.Equal(AccountingPeriod.Parse("2027-01"), next);
    }

    [Fact]
    public void StartAndEndDate_CoverWholeMonth()
    {
        var period = AccountingPeriod.Parse("2028-02");

        Assert.Equal(new DateOnly(2028, 2, 1), period.StartDate);
        Assert.Equal(new DateOnly(2028, 2, 29), period.EndDate);
    }

    [Fact]
    public void Contains_ChecksMonthBoundaries()
    {
        var period = AccountingPeriod.Parse("2026-10");

        Assert.True(period.Contains(new DateOnly(2026, 10, 1)));
        Assert.True(period.Contains(new DateOnly(2026, 10, 31)));
        Assert.False(period.Contains(new DateOnly(2026, 11, 1)));
    }
}

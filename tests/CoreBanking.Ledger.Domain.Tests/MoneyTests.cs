namespace CoreBanking.Ledger.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Of_WithMoreThanTwoDecimals_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Of(10.005m));

        Assert.Equal("money.invalid_scale", ex.Code);
    }

    [Fact]
    public void Of_WithTwoDecimals_KeepsAmount()
    {
        Assert.Equal(10.50m, Money.Of(10.50m).Amount);
    }

    [Fact]
    public void Of_Negative_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Of(-1m));

        Assert.Equal("money.negative", ex.Code);
    }

    [Fact]
    public void Add_SumsAmounts()
    {
        Assert.Equal(Money.Of(3.30m), Money.Of(1.10m) + Money.Of(2.20m));
    }

    [Fact]
    public void Subtract_BelowZero_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Of(1m) - Money.Of(2m));

        Assert.Equal("money.negative", ex.Code);
    }

    [Fact]
    public void Equality_ByValue()
    {
        Assert.Equal(Money.Of(5m), Money.Of(5.00m));
    }

    [Fact]
    public void Zero_IsZeroAndNotPositive()
    {
        Assert.True(Money.Zero.IsZero);
        Assert.False(Money.Zero.IsPositive);
    }

    [Fact]
    public void IsPositive_WhenAmountAboveZero()
    {
        Assert.True(Money.Of(0.01m).IsPositive);
    }

    [Fact]
    public void ComparisonOperators_CompareAmounts()
    {
        Assert.True(Money.Of(1m) < Money.Of(2m));
        Assert.True(Money.Of(2m) > Money.Of(1m));
    }
}

using CoreBanking.Ledger.Application.Queries;
using CoreBanking.Ledger.Application.Tests.Postings;
using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Tests.Queries;

public class GetBalanceHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private GetBalanceHandler Handler() => new(_ctx.Accounts, _ctx.Time);

    [Fact]
    public async Task GetBalance_AsOfPastDate_IgnoresLaterPostings()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        await _ctx.DepositAsync(customer, 100m, new DateOnly(2026, 11, 10));
        await _ctx.DepositAsync(customer, 50m, new DateOnly(2026, 11, 20));

        var view = await Handler().HandleAsync(new GetBalanceQuery(customer.Id, new DateOnly(2026, 11, 15)), Ct);

        Assert.Equal(100m, view.Balance);
    }

    [Fact]
    public async Task GetBalance_AsOfFuture_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");

        // 2026-11-25 is already today in UTC, but still tomorrow in São Paulo.
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(new GetBalanceQuery(customer.Id, new DateOnly(2026, 11, 25)), Ct));

        Assert.Equal("balance.as_of_in_future", ex.Code);
    }

    [Fact]
    public async Task GetBalance_WithoutAsOf_UsesToday()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        await _ctx.DepositAsync(customer, 100m);

        var view = await Handler().HandleAsync(new GetBalanceQuery(customer.Id, AsOf: null), Ct);

        Assert.Equal(PostingTestContext.Today, view.AsOf);
        Assert.Equal(100m, view.Balance);
    }
}

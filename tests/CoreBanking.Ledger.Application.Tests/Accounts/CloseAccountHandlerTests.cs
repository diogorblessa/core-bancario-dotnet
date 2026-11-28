using CoreBanking.Ledger.Application.Accounts;
using CoreBanking.Ledger.Application.Tests.Postings;
using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Tests.Accounts;

public class CloseAccountHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CloseAccount_WithBalance_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        await _ctx.DepositAsync(customer, 100m);
        var handler = new CloseAccountHandler(_ctx.Accounts, _ctx.UnitOfWork);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(new CloseAccountCommand(customer.Id), Ct));

        Assert.Equal("account.balance_not_zero", ex.Code);
    }
}

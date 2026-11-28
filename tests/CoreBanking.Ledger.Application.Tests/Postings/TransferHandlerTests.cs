using CoreBanking.Ledger.Application.Transfers;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Tests.Postings;

public class TransferHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransferHandler Handler() => new(_ctx.Accounts, _ctx.Workflow(), _ctx.Time);

    private static TransferCommand Transfer(Guid fromId, Guid toId) =>
        new(fromId, toId, 30m, BookingDate: null, Description: null);

    private Account CustomerWithBalance(string code, decimal balance)
    {
        var account = _ctx.OpenCustomerAccount(code);
        account.Apply(PostingSide.Credit, Money.Of(balance));
        return account;
    }

    [Fact]
    public async Task Transfer_MovesBalanceBetweenCustomers()
    {
        var from = CustomerWithBalance("4.1.1.000001", 100m);
        var to = _ctx.OpenCustomerAccount("4.1.1.000002");

        await Handler().HandleAsync(Transfer(from.Id, to.Id), Ct);

        Assert.Equal(Money.Of(70m), from.CurrentBalance);
        Assert.Equal(Money.Of(30m), to.CurrentBalance);
        Assert.Equal(1, _ctx.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Transfer_SameAccount_Throws()
    {
        var account = CustomerWithBalance("4.1.1.000001", 100m);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(Transfer(account.Id, account.Id), Ct));

        Assert.Equal("transfer.same_account", ex.Code);
    }

    [Fact]
    public async Task Transfer_ToClosedAccount_Throws()
    {
        var from = CustomerWithBalance("4.1.1.000001", 100m);
        var to = _ctx.OpenCustomerAccount("4.1.1.000002");
        to.Close();

        var ex = await Assert.ThrowsAsync<DomainException>(() => Handler().HandleAsync(Transfer(from.Id, to.Id), Ct));

        Assert.Equal("account.closed", ex.Code);
        Assert.Equal(0, _ctx.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Transfer_FromInternalAccount_Throws()
    {
        var to = _ctx.OpenCustomerAccount("4.1.1.000001");

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(Transfer(_ctx.Settlement.Id, to.Id), Ct));

        Assert.Equal("account.not_customer_account", ex.Code);
    }
}

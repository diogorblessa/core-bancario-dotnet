using CoreBanking.Ledger.Application.Withdrawals;
using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Tests.Postings;

public class WithdrawalHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WithdrawalHandler Handler() => new(_ctx.Accounts, _ctx.Workflow(), _ctx.Time);

    [Fact]
    public async Task Withdrawal_InsufficientBalance_Throws_AndDoesNotSave()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        var command = new WithdrawalCommand(customer.Id, 100m, BookingDate: null, "SAQ-001", Description: null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => Handler().HandleAsync(command, Ct));

        Assert.Equal("account.insufficient_balance", ex.Code);
        Assert.Equal(0, _ctx.UnitOfWork.SaveChangesCalls);
        Assert.Empty(_ctx.Entries.Entries);
    }
}

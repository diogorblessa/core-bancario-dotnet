using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Deposits;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Tests.Postings;

public class DepositHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DepositHandler Handler() => new(_ctx.Accounts, _ctx.Workflow(), _ctx.Time);

    private static DepositCommand Deposit(Guid accountId, string externalReference = "DEP-001") =>
        new(accountId, 100m, BookingDate: null, externalReference, Description: null);

    [Fact]
    public async Task Deposit_CreditsCustomerAndDebitsSettlement()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");

        var result = await Handler().HandleAsync(Deposit(customer.Id), Ct);

        Assert.Equal(Money.Of(100m), customer.CurrentBalance);
        var entry = Assert.Single(_ctx.Entries.Entries);
        Assert.Equal(result.JournalEntryId, entry.Id);
        Assert.Contains(entry.Postings, p => p.AccountId == _ctx.Settlement.Id && p.Side == PostingSide.Debit);
        Assert.Equal(1, _ctx.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Deposit_WithoutExternalReference_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(Deposit(customer.Id, externalReference: " "), Ct));

        Assert.Equal("entry.external_reference_required", ex.Code);
    }

    [Fact]
    public async Task Deposit_IntoInternalAccount_Throws()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(Deposit(_ctx.Settlement.Id), Ct));

        Assert.Equal("account.not_customer_account", ex.Code);
    }

    [Fact]
    public async Task Deposit_UnknownAccount_ThrowsNotFound()
    {
        var unknownId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Handler().HandleAsync(Deposit(unknownId), Ct));

        Assert.Equal(unknownId, ex.Id);
    }

    [Fact]
    public async Task Deposit_DefaultBookingDate_IsTodayInSaoPaulo()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");

        await Handler().HandleAsync(Deposit(customer.Id), Ct);

        Assert.Equal(PostingTestContext.Today, Assert.Single(_ctx.Entries.Entries).BookingDate);
    }
}

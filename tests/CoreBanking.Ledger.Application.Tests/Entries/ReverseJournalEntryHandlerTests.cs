using CoreBanking.Ledger.Application.Entries;
using CoreBanking.Ledger.Application.Tests.Postings;
using CoreBanking.Ledger.Application.Withdrawals;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Application.Tests.Entries;

public class ReverseJournalEntryHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ReverseJournalEntryHandler Handler() => new(_ctx.Entries, _ctx.Workflow(), _ctx.Time);

    [Fact]
    public async Task Reverse_RestoresBalances()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        var deposit = await _ctx.DepositAsync(customer, 100m);

        var result = await Handler().HandleAsync(new ReverseJournalEntryCommand(deposit.JournalEntryId), Ct);

        Assert.Equal(Money.Zero, customer.CurrentBalance);
        var reversal = Assert.Single(_ctx.Entries.Entries, e => e.Id == result.JournalEntryId);
        Assert.Equal(deposit.JournalEntryId, reversal.ReversalOf);
    }

    [Fact]
    public async Task Reverse_Twice_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        var deposit = await _ctx.DepositAsync(customer, 100m);
        await Handler().HandleAsync(new ReverseJournalEntryCommand(deposit.JournalEntryId), Ct);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(new ReverseJournalEntryCommand(deposit.JournalEntryId), Ct));

        Assert.Equal("entry.already_reversed", ex.Code);
    }

    [Fact]
    public async Task Reverse_WhenItWouldMakeCustomerNegative_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        var deposit = await _ctx.DepositAsync(customer, 100m);
        await new WithdrawalHandler(_ctx.Accounts, _ctx.Workflow(), _ctx.Time).HandleAsync(
            new WithdrawalCommand(customer.Id, 100m, BookingDate: null, "SAQ-001", Description: null),
            Ct);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(new ReverseJournalEntryCommand(deposit.JournalEntryId), Ct));

        Assert.Equal("account.insufficient_balance", ex.Code);
    }

    [Fact]
    public async Task Reverse_EntryFromClosedPeriod_BooksToday()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        var deposit = await _ctx.DepositAsync(customer, 100m, new DateOnly(2026, 10, 15));
        _ctx.Calendar.Calendar.Close(AccountingPeriod.Parse("2026-10"), PostingTestContext.Today);

        var result = await Handler().HandleAsync(new ReverseJournalEntryCommand(deposit.JournalEntryId), Ct);

        var reversal = Assert.Single(_ctx.Entries.Entries, e => e.Id == result.JournalEntryId);
        Assert.Equal(PostingTestContext.Today, reversal.BookingDate);
    }
}

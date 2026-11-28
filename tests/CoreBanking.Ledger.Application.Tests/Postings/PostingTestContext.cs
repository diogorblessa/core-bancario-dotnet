using CoreBanking.Ledger.Application.Deposits;
using CoreBanking.Ledger.Application.Postings;
using CoreBanking.Ledger.Application.Tests.Fakes;
using CoreBanking.Ledger.Domain.Accounts;

using Microsoft.Extensions.Time.Testing;

namespace CoreBanking.Ledger.Application.Tests.Postings;

public sealed class PostingTestContext
{
    // 2026-11-25T01:30Z is still 2026-11-24 in São Paulo (UTC-3).
    public static readonly DateOnly Today = new(2026, 11, 24);

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 25, 1, 30, 0, TimeSpan.Zero));

    public InMemoryJournalEntryRepository Entries { get; } = new();

    public InMemoryAccountRepository Accounts { get; }

    public InMemoryCustomerRepository Customers { get; } = new();

    public InMemoryAccountingCalendarRepository Calendar { get; } = new();

    public FakeUnitOfWork UnitOfWork { get; } = new();

    public Account Settlement { get; }

    public PostingTestContext()
    {
        Accounts = new InMemoryAccountRepository(Entries);
        Settlement = Account.CreateInternal(ChartOfAccounts.SettlementAccount, "Conta de liquidação", AccountNature.Asset);
        Accounts.Add(Settlement);
    }

    public Account OpenCustomerAccount(string code)
    {
        var account = Account.OpenCustomerDeposit(code, "Depósito à vista", Guid.NewGuid());
        Accounts.Add(account);
        return account;
    }

    public PostingWorkflow Workflow() => new(Accounts, Entries, Calendar, UnitOfWork, Time);

    public Task<PostingResult> DepositAsync(Account account, decimal amount, DateOnly? bookingDate = null) =>
        new DepositHandler(Accounts, Workflow(), Time).HandleAsync(
            new DepositCommand(account.Id, amount, bookingDate, "DEP-001", Description: null),
            TestContext.Current.CancellationToken);
}

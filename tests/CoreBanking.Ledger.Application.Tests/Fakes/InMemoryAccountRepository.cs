using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

// Posting totals are computed from the entries, as the SQL query will do in Task 9.
public sealed class InMemoryAccountRepository(InMemoryJournalEntryRepository entries) : IAccountRepository
{
    public List<Account> Accounts { get; } = [];

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Accounts.SingleOrDefault(a => a.Id == id));

    public Task<Account> GetByCodeAsync(string code, CancellationToken ct) =>
        Task.FromResult(Accounts.Single(a => a.Code == code));

    public Task<IReadOnlyList<Account>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Account>>(Accounts.Where(a => ids.Contains(a.Id)).ToList());

    public void Add(Account account) => Accounts.Add(account);

    public Task<PostingTotals> GetPostingTotalsAsync(Guid accountId, DateOnly asOf, CancellationToken ct)
    {
        var postings = entries.Entries
            .Where(e => e.BookingDate <= asOf)
            .SelectMany(e => e.Postings)
            .Where(p => p.AccountId == accountId)
            .ToList();

        var debits = postings.Where(p => p.Side == PostingSide.Debit).Sum(p => p.Amount.Amount);
        var credits = postings.Where(p => p.Side == PostingSide.Credit).Sum(p => p.Amount.Amount);

        return Task.FromResult(new PostingTotals(Money.Of(debits), Money.Of(credits)));
    }
}

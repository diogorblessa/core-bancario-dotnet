using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

public sealed class InMemoryJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> Entries { get; } = [];

    public void Add(JournalEntry entry) => Entries.Add(entry);

    public Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Entries.SingleOrDefault(e => e.Id == id));

    public Task<bool> HasReversalAsync(Guid entryId, CancellationToken ct) =>
        Task.FromResult(Entries.Any(e => e.ReversalOf == entryId));

    // Oldest first, like a bank statement.
    public Task<IReadOnlyList<AccountStatementLine>> GetStatementAsync(
        Guid accountId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var lines = Entries
            .OrderBy(e => e.BookingDate)
            .ThenBy(e => e.RecordedAt)
            .SelectMany(e => e.Postings
                .Where(p => p.AccountId == accountId)
                .Select(p => new AccountStatementLine(e.Id, e.BookingDate, e.Description, p.Side, p.Amount.Amount)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<AccountStatementLine>>(lines);
    }
}

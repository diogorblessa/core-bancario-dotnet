using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

public sealed class InMemoryJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> Entries { get; } = [];

    public void Add(JournalEntry entry) => Entries.Add(entry);

    // The methods below are implemented in Task 7, together with their handlers' tests.
    public Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<bool> HasReversalAsync(Guid entryId, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<AccountStatementLine>> GetStatementAsync(
        Guid accountId,
        int page,
        int pageSize,
        CancellationToken ct) =>
        throw new NotImplementedException();
}

using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Abstractions;

public interface IJournalEntryRepository
{
    void Add(JournalEntry entry);

    Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<bool> HasReversalAsync(Guid entryId, CancellationToken ct);

    Task<IReadOnlyList<AccountStatementLine>> GetStatementAsync(
        Guid accountId,
        int page,
        int pageSize,
        CancellationToken ct);
}

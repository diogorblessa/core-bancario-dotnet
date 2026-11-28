using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Account> GetByCodeAsync(string code, CancellationToken ct);

    Task<IReadOnlyList<Account>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);

    void Add(Account account);

    Task<PostingTotals> GetPostingTotalsAsync(Guid accountId, DateOnly asOf, CancellationToken ct);
}

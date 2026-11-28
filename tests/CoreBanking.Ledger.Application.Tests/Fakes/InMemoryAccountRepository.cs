using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

public sealed class InMemoryAccountRepository : IAccountRepository
{
    public List<Account> Accounts { get; } = [];

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Accounts.SingleOrDefault(a => a.Id == id));

    public Task<Account> GetByCodeAsync(string code, CancellationToken ct) =>
        Task.FromResult(Accounts.Single(a => a.Code == code));

    public Task<IReadOnlyList<Account>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Account>>(Accounts.Where(a => ids.Contains(a.Id)).ToList());

    public void Add(Account account) => Accounts.Add(account);

    // Implemented in Task 7, together with the balance query tests.
    public Task<PostingTotals> GetPostingTotalsAsync(Guid accountId, DateOnly asOf, CancellationToken ct) =>
        throw new NotImplementedException();
}

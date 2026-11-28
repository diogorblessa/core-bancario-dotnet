using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Queries;

public sealed class GetAccountEntriesHandler(IAccountRepository accounts, IJournalEntryRepository entries)
{
    private const int MaxPageSize = 200;

    public async Task<IReadOnlyList<AccountStatementLine>> HandleAsync(GetAccountEntriesQuery query, CancellationToken ct)
    {
        if (query.Page < 1)
        {
            throw new DomainException("query.invalid_page", "Page must be 1 or greater.");
        }

        if (query.PageSize is < 1 or > MaxPageSize)
        {
            throw new DomainException("query.invalid_page_size", $"Page size must be between 1 and {MaxPageSize}.");
        }

        if (await accounts.GetByIdAsync(query.AccountId, ct) is null)
        {
            throw new NotFoundException("Account", query.AccountId);
        }

        return await entries.GetStatementAsync(query.AccountId, query.Page, query.PageSize, ct);
    }
}

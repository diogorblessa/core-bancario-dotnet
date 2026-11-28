namespace CoreBanking.Ledger.Application.Queries;

public sealed record GetAccountEntriesQuery(Guid AccountId, int Page = 1, int PageSize = 50);

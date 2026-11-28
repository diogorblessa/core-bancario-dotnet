namespace CoreBanking.Ledger.Application.Queries;

public sealed record GetBalanceQuery(Guid AccountId, DateOnly? AsOf);

namespace CoreBanking.Ledger.Application.Queries;

public sealed record BalanceView(Guid AccountId, string AccountCode, DateOnly AsOf, decimal Balance);

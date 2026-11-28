namespace CoreBanking.Ledger.Application.Customers;

public sealed record CustomerCreated(Guid CustomerId, Guid AccountId, string AccountCode);

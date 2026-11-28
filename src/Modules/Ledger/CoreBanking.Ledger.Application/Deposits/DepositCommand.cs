namespace CoreBanking.Ledger.Application.Deposits;

public sealed record DepositCommand(
    Guid AccountId,
    decimal Amount,
    DateOnly? BookingDate,
    string ExternalReference,
    string? Description);

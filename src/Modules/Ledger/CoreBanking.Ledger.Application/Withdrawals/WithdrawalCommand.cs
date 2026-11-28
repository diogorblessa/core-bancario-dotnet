namespace CoreBanking.Ledger.Application.Withdrawals;

public sealed record WithdrawalCommand(
    Guid AccountId,
    decimal Amount,
    DateOnly? BookingDate,
    string ExternalReference,
    string? Description);

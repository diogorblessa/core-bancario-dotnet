namespace CoreBanking.Ledger.Application.Transfers;

public sealed record TransferCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateOnly? BookingDate,
    string? Description);

using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Abstractions;

public sealed record AccountStatementLine(
    Guid JournalEntryId,
    DateOnly BookingDate,
    string Description,
    PostingSide Side,
    decimal Amount);

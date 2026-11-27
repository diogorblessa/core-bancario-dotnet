using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Domain.Entries;

public sealed record PostingLine(Guid AccountId, PostingSide Side, Money Amount);

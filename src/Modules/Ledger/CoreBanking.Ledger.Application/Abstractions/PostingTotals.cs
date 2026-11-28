using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Abstractions;

public sealed record PostingTotals(Money Debits, Money Credits);

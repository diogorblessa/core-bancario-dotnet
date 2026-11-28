namespace CoreBanking.Ledger.Application.Abstractions;

public sealed class ConcurrencyConflictException(Exception? innerException = null)
    : Exception("The data was changed by another request.", innerException);

namespace CoreBanking.Ledger.Application.Abstractions;

public interface IUnitOfWork
{
    // Throws ConcurrencyConflictException when another request changed the same data.
    Task SaveChangesAsync(CancellationToken ct);
}

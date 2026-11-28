using CoreBanking.Ledger.Application.Abstractions;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }
}

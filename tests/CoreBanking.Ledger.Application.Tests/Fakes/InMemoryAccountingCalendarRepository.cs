using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

public sealed class InMemoryAccountingCalendarRepository : IAccountingCalendarRepository
{
    public AccountingCalendar Calendar { get; } = new();

    public Task<AccountingCalendar> GetAsync(CancellationToken ct) => Task.FromResult(Calendar);
}

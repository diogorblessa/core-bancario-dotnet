using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Application.Abstractions;

public interface IAccountingCalendarRepository
{
    Task<AccountingCalendar> GetAsync(CancellationToken ct);
}

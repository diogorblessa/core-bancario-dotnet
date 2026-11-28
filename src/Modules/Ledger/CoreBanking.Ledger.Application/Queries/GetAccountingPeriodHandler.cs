using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Periods;
using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Application.Queries;

public sealed class GetAccountingPeriodHandler(IAccountingCalendarRepository calendars)
{
    public async Task<AccountingPeriodView> HandleAsync(GetAccountingPeriodQuery query, CancellationToken ct)
    {
        var period = AccountingPeriod.Parse(query.Period);
        var calendar = await calendars.GetAsync(ct);

        return new AccountingPeriodView(period.ToString(), calendar.IsClosed(period));
    }
}

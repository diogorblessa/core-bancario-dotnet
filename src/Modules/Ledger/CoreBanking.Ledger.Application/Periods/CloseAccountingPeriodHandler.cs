using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Periods;

namespace CoreBanking.Ledger.Application.Periods;

public sealed class CloseAccountingPeriodHandler(
    IAccountingCalendarRepository calendars,
    IUnitOfWork unitOfWork,
    TimeProvider time)
{
    public async Task<AccountingPeriodView> HandleAsync(CloseAccountingPeriodCommand command, CancellationToken ct)
    {
        var period = AccountingPeriod.Parse(command.Period);
        var calendar = await calendars.GetAsync(ct);

        calendar.Close(period, LedgerClock.Today(time));
        await unitOfWork.SaveChangesAsync(ct);

        return new AccountingPeriodView(period.ToString(), IsClosed: true);
    }
}

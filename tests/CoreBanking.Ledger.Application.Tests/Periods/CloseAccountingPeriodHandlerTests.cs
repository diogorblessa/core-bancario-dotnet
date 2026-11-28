using CoreBanking.Ledger.Application.Periods;
using CoreBanking.Ledger.Application.Tests.Postings;

namespace CoreBanking.Ledger.Application.Tests.Periods;

public class CloseAccountingPeriodHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ClosePeriod_ReturnsClosedView()
    {
        var handler = new CloseAccountingPeriodHandler(_ctx.Calendar, _ctx.UnitOfWork, _ctx.Time);

        var view = await handler.HandleAsync(new CloseAccountingPeriodCommand("2026-10"), Ct);

        Assert.Equal(new AccountingPeriodView("2026-10", IsClosed: true), view);
        Assert.Equal(1, _ctx.UnitOfWork.SaveChangesCalls);
    }
}

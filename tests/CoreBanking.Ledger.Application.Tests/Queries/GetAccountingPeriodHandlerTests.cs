using CoreBanking.Ledger.Application.Periods;
using CoreBanking.Ledger.Application.Queries;
using CoreBanking.Ledger.Application.Tests.Postings;

namespace CoreBanking.Ledger.Application.Tests.Queries;

public class GetAccountingPeriodHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetPeriod_OpenPeriod_ReturnsNotClosed()
    {
        var handler = new GetAccountingPeriodHandler(_ctx.Calendar);

        var view = await handler.HandleAsync(new GetAccountingPeriodQuery("2026-11"), Ct);

        Assert.Equal(new AccountingPeriodView("2026-11", IsClosed: false), view);
    }
}

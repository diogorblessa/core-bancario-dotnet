using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Queries;
using CoreBanking.Ledger.Application.Tests.Postings;
using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Tests.Queries;

public class GetAccountEntriesHandlerTests
{
    private readonly PostingTestContext _ctx = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private GetAccountEntriesHandler Handler() => new(_ctx.Accounts, _ctx.Entries);

    [Fact]
    public async Task GetEntries_PageSizeAbove200_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(new GetAccountEntriesQuery(customer.Id, Page: 1, PageSize: 201), Ct));

        Assert.Equal("query.invalid_page_size", ex.Code);
    }

    [Fact]
    public async Task GetEntries_PageBelowOne_Throws()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => Handler().HandleAsync(new GetAccountEntriesQuery(customer.Id, Page: 0), Ct));

        Assert.Equal("query.invalid_page", ex.Code);
    }

    [Fact]
    public async Task GetEntries_UnknownAccount_ThrowsNotFound()
    {
        var unknownId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => Handler().HandleAsync(new GetAccountEntriesQuery(unknownId), Ct));

        Assert.Equal(unknownId, ex.Id);
    }

    [Fact]
    public async Task GetEntries_ReturnsAccountLinesInBookingOrder()
    {
        var customer = _ctx.OpenCustomerAccount("4.1.1.000001");
        await _ctx.DepositAsync(customer, 50m, new DateOnly(2026, 11, 20));
        await _ctx.DepositAsync(customer, 100m, new DateOnly(2026, 11, 10));

        var lines = await Handler().HandleAsync(new GetAccountEntriesQuery(customer.Id), Ct);

        Assert.Equal([new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 20)], lines.Select(l => l.BookingDate));
        Assert.Equal([100m, 50m], lines.Select(l => l.Amount));
    }
}

using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace CoreBanking.Ledger.IntegrationTests;

public class HealthEndpointTests
{
    [Fact]
    public async Task Get_Health_ReturnsOk()
    {
        using var factory = new WebApplicationFactory<Program>();

        var response = await factory.CreateClient().GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

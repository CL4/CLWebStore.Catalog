using System.Net;
using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLWebStore.Catalog.IntegrationTests.SmokeTests;

[Collection("Catalog Integration")]
public sealed class ApiBootSmokeTest
{
    private readonly CatalogWebApplicationFactory _factory;

    public ApiBootSmokeTest(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheckEndpoint_ReturnsOk_WhenApplicationStarts()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

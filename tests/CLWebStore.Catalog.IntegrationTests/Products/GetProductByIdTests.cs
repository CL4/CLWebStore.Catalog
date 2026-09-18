using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class GetProductByIdTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CatalogWebApplicationFactory _factory;

    public GetProductByIdTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProductById_ReturnsOk_WhenProductExists()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        var product = fixture.First(p => p.Sku == "MON-ASUS-PG32UCDM");

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, [product]);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductDto>(content, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Sku.Should().Be(product.Sku);
        result.Name.Should().Be(product.Name);
        result.PriceAmount.Should().Be(product.PriceAmount);
        result.PriceCurrency.Should().Be(product.PriceCurrency);
        result.CategoryIds.Should().BeEquivalentTo(product.CategoryIds);
        result.RelatedProductIds.Should().BeEquivalentTo(product.RelatedProductIds);

        result.Images.Should().NotBeNull();
        result.Images.Select(i => new { i.Url, i.AltText, i.IsPrimary })
            .Should()
            .BeEquivalentTo(product.Images.Select(i => new { i.Url, i.AltText, i.IsPrimary }));
    }

    [Fact]
    public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        await _factory.ResetDatabaseStateAsync();

        var missingProductId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/{missingProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Resource Not Found");
        problemDetails.Detail.Should().Contain(missingProductId.ToString());
    }
}

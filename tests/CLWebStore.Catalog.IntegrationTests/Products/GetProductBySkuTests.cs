using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class GetProductBySkuTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CatalogWebApplicationFactory _factory;

    public GetProductBySkuTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProductBySku_ReturnsOk_WhenProductExists()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        var product = fixture.First(p => p.Sku == "MON-ASUS-PG32UCDM");

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, [product]);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/sku/{product.Sku}");

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
    public async Task GetProductBySku_ReturnsNotFound_WhenSkuDoesNotExist()
    {
        await _factory.ResetDatabaseStateAsync();

        const string missingSku = "SKU-DOES-NOT-EXIST";

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/sku/{missingSku}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class SearchProductsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CatalogWebApplicationFactory _factory;

    public SearchProductsTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SearchProducts_ReturnsMatchingProducts_WhenNameContainsPartialTerm_AndOrdersByName()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        const string searchTerm = "OLED";

        var expectedProducts = fixture
            .Where(product => product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(product => product.Name)
            .ToArray();

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, fixture);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/search?searchTerm={searchTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().HaveCount(expectedProducts.Length);
        results!.Select(product => product.Id)
            .Should()
            .Equal(expectedProducts.Select(product => product.Id));

        foreach (var expectedProduct in expectedProducts)
        {
            var actualProduct = results.Single(product => product.Id == expectedProduct.Id);

            actualProduct.Sku.Should().Be(expectedProduct.Sku);
            actualProduct.Name.Should().Be(expectedProduct.Name);
            actualProduct.PriceAmount.Should().Be(expectedProduct.PriceAmount);
            actualProduct.PriceCurrency.Should().Be(expectedProduct.PriceCurrency);
            actualProduct.CategoryIds.Should().BeEquivalentTo(expectedProduct.CategoryIds);
            actualProduct.RelatedProductIds.Should().BeEquivalentTo(expectedProduct.RelatedProductIds);
            actualProduct.Images.Select(image => new { image.Url, image.AltText, image.IsPrimary })
                .Should()
                .BeEquivalentTo(expectedProduct.Images.Select(image => new { image.Url, image.AltText, image.IsPrimary }));
        }

        var allIds = fixture.Select(product => product.Id).ToHashSet();
        var expectedIds = expectedProducts.Select(product => product.Id).ToHashSet();

        results.Select(product => product.Id)
            .Should()
            .OnlyContain(id => expectedIds.Contains(id));

        results.Select(product => product.Id)
            .Should()
            .NotContain(id => !expectedIds.Contains(id));

        var nonMatchingProductIds = allIds.Where(id => !expectedIds.Contains(id)).Take(1).ToArray();
        results.Select(product => product.Id)
            .Should()
            .NotContain(nonMatchingProductIds);
    }

    [Fact]
    public async Task SearchProducts_ReturnsMatchingProducts_WhenSkuContainsPartialTerm()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        const string searchTerm = "RTX";

        var expectedProducts = fixture
            .Where(product => product.Sku.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(product => product.Name)
            .ToArray();

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, fixture);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/search?searchTerm={searchTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().HaveCount(expectedProducts.Length);
        results!.Select(product => product.Id)
            .Should()
            .Equal(expectedProducts.Select(product => product.Id));

        results.Select(product => product.Id)
            .Should()
            .BeEquivalentTo(expectedProducts.Select(product => product.Id));

        results.Select(product => product.Sku)
            .Should()
            .ContainSingle(sku => sku == expectedProducts[0].Sku);
    }

    [Fact]
    public async Task SearchProducts_IsCaseInsensitive_WhenSearchTermCasingChanges()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        const string searchTerm = "RyZen";

        var expectedProducts = fixture
            .Where(product => product.Name.Contains("RYZEN", StringComparison.OrdinalIgnoreCase))
            .OrderBy(product => product.Name)
            .ToArray();

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, fixture);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/search?searchTerm={searchTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().HaveCount(expectedProducts.Length);
        results!.Select(product => product.Id)
            .Should()
            .Equal(expectedProducts.Select(product => product.Id));
    }

    [Fact]
    public async Task SearchProducts_ReturnsOk_WithEmptyCollection_WhenNoProductsMatch()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        const string searchTerm = "NO-PRODUCT-SHOULD-MATCH-THIS-TERM";

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, fixture);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/search?searchTerm={searchTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }
}

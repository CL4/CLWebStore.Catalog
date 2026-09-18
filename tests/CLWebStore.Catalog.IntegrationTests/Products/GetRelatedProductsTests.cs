using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class GetRelatedProductsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CatalogWebApplicationFactory _factory;

    public GetRelatedProductsTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRelatedProducts_ReturnsOk_WithExpectedProducts_WhenProductHasRelatedProducts()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        var sourceProduct = fixture.First(product => product.RelatedProductIds.Length > 0);
        var expectedProductIds = sourceProduct.RelatedProductIds;
        var expectedProducts = fixture
            .Where(product => expectedProductIds.Contains(product.Id))
            .ToArray();

        var seedProducts = new[] { sourceProduct }
            .Concat(expectedProducts)
            .DistinctBy(product => product.Id)
            .ToArray();

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, seedProducts);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/{sourceProduct.Id}/related");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().HaveCount(expectedProducts.Length);
        results!.Select(product => product.Id)
            .Should()
            .BeEquivalentTo(expectedProducts.Select(product => product.Id));

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
    }

    [Fact]
    public async Task GetRelatedProducts_ReturnsOk_WithEmptyCollection_WhenProductHasNoRelatedProducts()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        var productWithNoRelatedProducts = fixture.First(product => product.RelatedProductIds.Length == 0);

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, [productWithNoRelatedProducts]);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/{productWithNoRelatedProducts.Id}/related");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRelatedProducts_ExcludesUnrelatedProducts_FromResults()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        var sourceProduct = fixture.First(product => product.RelatedProductIds.Length > 0);
        var expectedProductIds = sourceProduct.RelatedProductIds;
        var expectedProducts = fixture
            .Where(product => expectedProductIds.Contains(product.Id))
            .ToArray();
        var unrelatedProduct = fixture
            .First(product => product.Id != sourceProduct.Id && !expectedProductIds.Contains(product.Id));

        var seedProducts = new[] { sourceProduct }
            .Concat(expectedProducts)
            .Append(unrelatedProduct)
            .DistinctBy(product => product.Id)
            .ToArray();

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, seedProducts);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/{sourceProduct.Id}/related");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results!.Select(product => product.Id)
            .Should()
            .BeEquivalentTo(expectedProducts.Select(product => product.Id));
        results.Select(product => product.Id)
            .Should()
            .NotContain(unrelatedProduct.Id);
    }
}

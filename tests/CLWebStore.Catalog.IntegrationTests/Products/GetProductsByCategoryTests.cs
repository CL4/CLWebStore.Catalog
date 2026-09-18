using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class GetProductsByCategoryTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CatalogWebApplicationFactory _factory;

    public GetProductsByCategoryTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsOk_WithMatchingProducts_WhenCategoryExists()
    {
        await _factory.ResetDatabaseStateAsync();

        var fixture = await ProductFixtureLoader.LoadAsync();
        var categoryId = fixture
            .SelectMany(product => product.CategoryIds)
            .GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .First();

        var expectedProducts = fixture
            .Where(product => product.CategoryIds.Contains(categoryId))
            .ToArray();

        await PostgresTestDataSeeder.SeedAsync(_factory.PostgresConnectionString, expectedProducts);

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/category/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results!.Should().HaveCount(expectedProducts.Length);
        results.Select(product => product.Id)
            .Should()
            .BeEquivalentTo(expectedProducts.Select(product => product.Id));

        results.Select(product => product.Sku)
            .Should()
            .BeEquivalentTo(expectedProducts.Select(product => product.Sku));
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsOk_WithEmptyCollection_WhenCategoryHasNoMatches()
    {
        await _factory.ResetDatabaseStateAsync();

        var categoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/Products/category/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);

        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CLWebStore.Catalog.API.Contracts.V1.Requests;
using CLWebStore.Catalog.API.Contracts.V1.Responses;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class UpdateProductErrorTests
{
    private readonly CatalogWebApplicationFactory _factory;

    public UpdateProductErrorTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateProduct_WithNonexistentProduct_ReturnsNotFound()
    {
        await _factory.ResetDatabaseStateAsync();

        var id = Guid.NewGuid();
        var request = UpdateProductRequestBuilder.New()
            .WithName("Valid Product Name")
            .WithPriceAmount(10.5m)
            .WithPriceCurrency("USD")
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/v1/Products/{id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = payload.RootElement;

        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(404);

        root.TryGetProperty("title", out var title).Should().BeTrue();
        title.GetString().Should().Be("Resource Not Found");
    }

    [Fact]
    public async Task UpdateProduct_WithEmptyName_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseStateAsync();

        var productId = await CreateProductAsync();
        var request = UpdateProductRequestBuilder.New()
            .WithName(string.Empty)
            .WithPriceAmount(10.5m)
            .WithPriceCurrency("USD")
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/v1/Products/{productId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertValidationError(payload.RootElement, "Name");

        var product = await GetProductDocumentAsync(productId);
        product.Name.Should().NotBeEmpty();
        product.Name.Should().NotBe(" ");
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidPriceAmount_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseStateAsync();

        var productId = await CreateProductAsync();
        var request = UpdateProductRequestBuilder.New()
            .WithName("Valid Product Name")
            .WithPriceAmount(0m)
            .WithPriceCurrency("USD")
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/v1/Products/{productId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertValidationError(payload.RootElement, "PriceAmount");

        var product = await GetProductDocumentAsync(productId);
        product.PriceAmount.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidPriceCurrency_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseStateAsync();

        var productId = await CreateProductAsync();
        var request = UpdateProductRequestBuilder.New()
            .WithName("Valid Product Name")
            .WithPriceAmount(10.5m)
            .WithPriceCurrency("US")
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/v1/Products/{productId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertValidationError(payload.RootElement, "PriceCurrency");

        var product = await GetProductDocumentAsync(productId);
        product.PriceCurrency.Should().Be("USD");
    }

    [Fact]
    public async Task UpdateProduct_WithSelfRelatedProduct_ReturnsBadRequest_AndDoesNotMutateProduct()
    {
        await _factory.ResetDatabaseStateAsync();

        var productId = await CreateProductAsync();
        var productBefore = await GetProductDocumentAsync(productId);
        var request = UpdateProductRequestBuilder.New()
            .WithName("Updated Product")
            .WithPriceAmount(12.5m)
            .WithPriceCurrency("USD")
            .WithRelatedProductIds(productId)
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/v1/Products/{productId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = payload.RootElement;

        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(400);

        root.TryGetProperty("title", out var title).Should().BeTrue();
        title.GetString().Should().Be("Domain Rule Violation");

        root.TryGetProperty("detail", out var detail).Should().BeTrue();
        detail.GetString().Should().Contain("Product cannot relate to itself");

        var productAfter = await GetProductDocumentAsync(productId);
        productAfter.Name.Should().Be(productBefore.Name);
        productAfter.PriceAmount.Should().Be(productBefore.PriceAmount);
        productAfter.PriceCurrency.Should().Be(productBefore.PriceCurrency);
        productAfter.RelatedProductIds.Should().BeEquivalentTo(productBefore.RelatedProductIds);
    }

    private async Task<Guid> CreateProductAsync()
    {
        var sku = $"TEST-{Guid.NewGuid():N}";
        var createRequest = CreateProductRequestBuilder.New()
            .WithSku(sku)
            .WithName("Original Product")
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("USD")
            .WithCategoryIds()
            .WithRelatedProductIds()
            .WithImages()
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Products", createRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ProductCreatedResponse>();
        created.Should().NotBeNull();

        return created!.Id;
    }

    private async Task<ProductDocument> GetProductDocumentAsync(Guid productId)
    {
        var container = _factory.GetCatalogContainer();
        var itemResponse = await container.ReadItemAsync<ProductDocument>(
            productId.ToString(),
            new PartitionKey(productId.ToString()));

        return itemResponse.Resource;
    }

    private static void AssertValidationError(JsonElement root, string expectedPropertyName)
    {
        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(400);

        root.TryGetProperty("title", out var title).Should().BeTrue();
        title.GetString().Should().Be("Validation Failed");

        root.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.ValueKind.Should().Be(JsonValueKind.Array);

        var validationErrors = errors.EnumerateArray().ToList();
        validationErrors.Any(e =>
        {
            if (!e.TryGetProperty("propertyName", out var propertyName))
            {
                return false;
            }

            return MatchesExpectedProperty(propertyName.GetString(), expectedPropertyName);
        }).Should().BeTrue();
    }

    private static bool MatchesExpectedProperty(string? propertyName, string expectedPropertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return false;
        }

        return propertyName.Equals(expectedPropertyName, StringComparison.Ordinal)
            || propertyName.EndsWith($".{expectedPropertyName}", StringComparison.Ordinal);
    }
}

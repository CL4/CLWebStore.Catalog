using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CLWebStore.Catalog.API.Contracts.V1.Requests;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class CreateProductValidationTests
{
    private readonly CatalogWebApplicationFactory _factory;

    public CreateProductValidationTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_WithMissingSku_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku(string.Empty)
            .WithName("Valid Product")
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("USD")
            .Build();

        await AssertInvalidRequestAsync(request, "Sku");
    }

    [Fact]
    public async Task CreateProduct_WithMissingName_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku($"SKU-{Guid.NewGuid():N}")
            .WithName(string.Empty)
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("USD")
            .Build();

        await AssertInvalidRequestAsync(request, "Name");
    }

    [Fact]
    public async Task CreateProduct_WithNonPositivePriceAmount_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku($"SKU-{Guid.NewGuid():N}")
            .WithName("Valid Product")
            .WithPriceAmount(0m)
            .WithPriceCurrency("USD")
            .Build();

        await AssertInvalidRequestAsync(request, "PriceAmount");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidPriceCurrencyLength_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku($"SKU-{Guid.NewGuid():N}")
            .WithName("Valid Product")
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("US")
            .Build();

        await AssertInvalidRequestAsync(request, "PriceCurrency");
    }

    [Fact]
    public async Task CreateProduct_WithSkuLongerThanMaximum_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku(new string('A', 51))
            .WithName("Valid Product")
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("USD")
            .Build();

        await AssertInvalidRequestAsync(request, "Sku");
    }

    [Fact]
    public async Task CreateProduct_WithNameLongerThanMaximum_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku($"SKU-{Guid.NewGuid():N}")
            .WithName(new string('N', 201))
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("USD")
            .Build();

        await AssertInvalidRequestAsync(request, "Name");
    }

    [Fact]
    public async Task CreateProduct_WithEmptyImageUrl_ReturnsBadRequest_AndDoesNotPersistAnything()
    {
        await _factory.ResetDatabaseStateAsync();

        var request = CreateProductRequestBuilder.New()
            .WithSku($"SKU-{Guid.NewGuid():N}")
            .WithName("Valid Product")
            .WithPriceAmount(99.99m)
            .WithPriceCurrency("USD")
            .WithImages(new ProductImageRequest(string.Empty, "Product image", true))
            .Build();

        await AssertInvalidRequestAsync(request, "Url");
    }

    private async Task AssertInvalidRequestAsync(CreateProductRequest request, string expectedPropertyName)
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = payload.RootElement;

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

        var container = _factory.GetCatalogContainer();
        var productsBySku = string.IsNullOrWhiteSpace(request.Sku)
            ? await QueryDocumentsAsync<ProductDocument>(container, "SELECT * FROM c WHERE c.type = 'Product'")
            : await QueryDocumentsAsync<ProductDocument>(
                container,
                "SELECT * FROM c WHERE c.sku = @sku",
                new Dictionary<string, object> { ["@sku"] = request.Sku });

        productsBySku.Should().BeEmpty();

        var outboxItems = string.IsNullOrWhiteSpace(request.Sku)
            ? await QueryDocumentsAsync<OutboxDocument>(container, "SELECT * FROM c WHERE c.type = 'OutboxEvent' AND c.eventType = 'ProductCreatedEvent'")
            : await QueryDocumentsAsync<OutboxDocument>(
                container,
                "SELECT * FROM c WHERE c.type = 'OutboxEvent' AND c.eventType = 'ProductCreatedEvent' AND CONTAINS(c.payload, @sku)",
                new Dictionary<string, object> { ["@sku"] = request.Sku });

        outboxItems.Should().BeEmpty();
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

    private static async Task<List<T>> QueryDocumentsAsync<T>(Container container, string queryText, Dictionary<string, object>? parameters = null)
        where T : class
    {
        var query = new QueryDefinition(queryText);

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                query = query.WithParameter(parameter.Key, parameter.Value);
            }
        }

        var iterator = container.GetItemQueryIterator<T>(query);
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}

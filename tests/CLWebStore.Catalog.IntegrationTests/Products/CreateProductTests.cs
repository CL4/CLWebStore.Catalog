using CLWebStore.Catalog.API.Contracts.V1.Requests;
using CLWebStore.Catalog.API.Contracts.V1.Responses;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using CLWebStore.Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CLWebStore.Catalog.IntegrationTests.Products;

[Collection("Catalog Integration")]
public sealed class CreateProductTests
{
    private readonly CatalogWebApplicationFactory _factory;

    public CreateProductTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreated_AndPersistsProduct()
    {
        await _factory.ResetDatabaseStateAsync();

        var sku = $"TEST-{Guid.NewGuid():N}";

        var request = CreateProductRequestBuilder.New()
            .WithSku(sku)
            .WithName("Minimal Test Product")
            .WithPriceAmount(123.45m)
            .WithPriceCurrency("USD")
            .WithCategoryIds()
            .WithRelatedProductIds()
            .WithImages()
            .Build();

        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ProductCreatedResponse>();

        created.Should().NotBeNull();
        created!.Id.Should().NotBe(Guid.Empty);

        var productId = created.Id;
        var container = _factory.GetCatalogContainer();
        var itemResponse = await container.ReadItemAsync<ProductDocument>(
            productId.ToString(),
            new PartitionKey(productId.ToString()));

        var productDocument = itemResponse.Resource;

        productDocument.Should().NotBeNull();
        productDocument.Id.Should().Be(productId.ToString());
        productDocument.PartitionKey.Should().Be(productId.ToString());
        productDocument.Type.Should().Be("Product");
        productDocument.Sku.Should().Be(sku.ToUpperInvariant());
        productDocument.Name.Should().Be("Minimal Test Product");
        productDocument.PriceAmount.Should().Be(123.45m);
        productDocument.PriceCurrency.Should().Be("USD");
        productDocument.CategoryIds.Should().BeEmpty();
        productDocument.RelatedProductIds.Should().BeEmpty();
        productDocument.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithMinimalData_PersistsProductAndSingleCreatedOutboxEvent()
    {
        await _factory.ResetDatabaseStateAsync();

        var sku = $"TEST-{Guid.NewGuid():N}";
        var request = CreateProductRequestBuilder.New()
            .WithSku(sku)
            .WithName("Minimal Test Product")
            .WithPriceAmount(123.45m)
            .WithPriceCurrency("USD")
            .WithCategoryIds()
            .WithRelatedProductIds()
            .WithImages()
            .Build();

        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ProductCreatedResponse>();
        created.Should().NotBeNull();

        var productId = created!.Id;
        var container = _factory.GetCatalogContainer();
        var productDocument = await ReadProductDocumentAsync(container, productId);

        productDocument.Id.Should().Be(productId.ToString());
        productDocument.PartitionKey.Should().Be(productId.ToString());
        productDocument.Type.Should().Be("Product");
        productDocument.Sku.Should().Be(sku.ToUpperInvariant());
        productDocument.Name.Should().Be("Minimal Test Product");
        productDocument.PriceAmount.Should().Be(123.45m);
        productDocument.PriceCurrency.Should().Be("USD");
        productDocument.CategoryIds.Should().BeEmpty();
        productDocument.RelatedProductIds.Should().BeEmpty();
        productDocument.Images.Should().BeEmpty();

        var outboxDocuments = await ReadOutboxDocumentsAsync(container, productId.ToString());
        outboxDocuments.Should().HaveCount(1);
        outboxDocuments.Should().OnlyContain(x => x.PartitionKey == productId.ToString());
        outboxDocuments[0].EventType.Should().Be("ProductCreatedEvent");

        using var payload = JsonDocument.Parse(outboxDocuments[0].Payload);
        var root = payload.RootElement;

        root.GetProperty("ProductId").GetString().Should().Be(productId.ToString());
        root.GetProperty("Sku").GetString().Should().Be(sku.ToUpperInvariant());
        root.GetProperty("Name").GetString().Should().Be("Minimal Test Product");
        root.GetProperty("PriceAmount").GetDecimal().Should().Be(123.45m);
        root.GetProperty("PriceCurrency").GetString().Should().Be("USD");
        root.GetProperty("CategoryIds").GetArrayLength().Should().Be(0);
        root.GetProperty("RelatedProductIds").GetArrayLength().Should().Be(0);
        root.GetProperty("Images").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task CreateProduct_WithAssociations_PersistsProductAndExpectedOutboxEvents()
    {
        await _factory.ResetDatabaseStateAsync();

        var categoryIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var relatedProductIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var imageUrls = new[]
        {
            "https://example.com/images/first.jpg",
            "https://example.com/images/second.jpg",
        };

        var request = CreateProductRequestBuilder.New()
            .WithSku($"TEST-{Guid.NewGuid():N}")
            .WithName("Associated Product")
            .WithPriceAmount(149.99m)
            .WithPriceCurrency("EUR")
            .WithCategoryIds(categoryIds[0], categoryIds[1])
            .WithRelatedProductIds(relatedProductIds[0], relatedProductIds[1])
            .WithImages(
                new ProductImageRequest(imageUrls[0], "First image", true),
                new ProductImageRequest(imageUrls[1], "Second image", false))
            .Build();

        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ProductCreatedResponse>();
        created.Should().NotBeNull();

        var productId = created!.Id;
        var container = _factory.GetCatalogContainer();
        var productDocument = await ReadProductDocumentAsync(container, productId);

        productDocument.Id.Should().Be(productId.ToString());
        productDocument.PartitionKey.Should().Be(productId.ToString());
        productDocument.Type.Should().Be("Product");
        productDocument.CategoryIds.Should().BeEquivalentTo(categoryIds);
        productDocument.RelatedProductIds.Should().BeEquivalentTo(relatedProductIds);
        productDocument.Images.Select(i => i.Url).Should().BeEquivalentTo(imageUrls);

        var outboxDocuments = await ReadOutboxDocumentsAsync(container, productId.ToString());
        outboxDocuments.Should().HaveCount(7);
        outboxDocuments.Should().OnlyContain(x => x.PartitionKey == productId.ToString());
        outboxDocuments.Count(x => x.EventType == "ProductCreatedEvent").Should().Be(1);
        outboxDocuments.Count(x => x.EventType == "ProductUpdatedEvent").Should().Be(6);

        var createdEvent = outboxDocuments.Single(x => x.EventType == "ProductCreatedEvent");
        using var createdPayload = JsonDocument.Parse(createdEvent.Payload);
        createdPayload.RootElement.GetProperty("ProductId").GetString().Should().Be(productId.ToString());
        createdPayload.RootElement.GetProperty("CategoryIds").GetArrayLength().Should().Be(0);
        createdPayload.RootElement.GetProperty("RelatedProductIds").GetArrayLength().Should().Be(0);
        createdPayload.RootElement.GetProperty("Images").GetArrayLength().Should().Be(0);

        var updatedEventPayloads = outboxDocuments
            .Where(x => x.EventType == "ProductUpdatedEvent")
            .Select(x => JsonDocument.Parse(x.Payload))
            .ToList();

        updatedEventPayloads.Should().HaveCount(6);

        var payloadCategories = updatedEventPayloads
            .SelectMany(p => p.RootElement.GetProperty("CategoryIds").EnumerateArray())
            .Select(v => Guid.Parse(v.GetString()!))
            .Distinct()
            .ToList();
        payloadCategories.Should().BeEquivalentTo(categoryIds);

        var payloadRelated = updatedEventPayloads
            .SelectMany(p => p.RootElement.GetProperty("RelatedProductIds").EnumerateArray())
            .Select(v => Guid.Parse(v.GetString()!))
            .Distinct()
            .ToList();
        payloadRelated.Should().BeEquivalentTo(relatedProductIds);

        var payloadUrls = updatedEventPayloads
            .SelectMany(p => p.RootElement.GetProperty("Images").EnumerateArray())
            .Select(i => i.GetProperty("Url").GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
        payloadUrls.Should().BeEquivalentTo(imageUrls);
    }

    private static async Task<ProductDocument> ReadProductDocumentAsync(Container container, Guid productId)
    {
        var response = await container.ReadItemAsync<ProductDocument>(
            productId.ToString(),
            new PartitionKey(productId.ToString()));

        return response.Resource;
    }

    private static async Task<List<OutboxDocument>> ReadOutboxDocumentsAsync(Container container, string partitionKey)
    {
        var iterator = container.GetItemQueryIterator<OutboxDocument>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey AND c.type = 'OutboxEvent'")
                .WithParameter("@partitionKey", partitionKey));

        var documents = new List<OutboxDocument>();
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            documents.AddRange(page);
        }

        return documents;
    }
}

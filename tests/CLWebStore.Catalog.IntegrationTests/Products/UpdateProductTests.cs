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
public sealed class UpdateProductTests
{
    private readonly CatalogWebApplicationFactory _factory;

    public UpdateProductTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateProduct_WhenValidRequest_ReturnsNoContent_AndPersistsUpdatedProduct()
    {
        await _factory.ResetDatabaseStateAsync();

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

        var createResponse = await client.PostAsJsonAsync("/api/v1/Products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductCreatedResponse>();
        created.Should().NotBeNull();

        var productId = created!.Id;
        var container = _factory.GetCatalogContainer();

        var updateRequest = UpdateProductRequestBuilder.New()
            .WithName("Updated Product Name")
            .WithPriceAmount(149.99m)
            .WithPriceCurrency("EUR")
            .WithCategoryIds()
            .WithRelatedProductIds()
            .WithImages()
            .Build();

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/Products/{productId}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var itemResponse = await container.ReadItemAsync<ProductDocument>(
            productId.ToString(),
            new PartitionKey(productId.ToString()));

        var productDocument = itemResponse.Resource;

        productDocument.Should().NotBeNull();
        productDocument.Id.Should().Be(productId.ToString());
        productDocument.PartitionKey.Should().Be(productId.ToString());
        productDocument.Type.Should().Be("Product");
        productDocument.Sku.Should().Be(sku.ToUpperInvariant());
        productDocument.Name.Should().Be("Updated Product Name");
        productDocument.PriceAmount.Should().Be(149.99m);
        productDocument.PriceCurrency.Should().Be("EUR");
        productDocument.CategoryIds.Should().BeEmpty();
        productDocument.RelatedProductIds.Should().BeEmpty();
        productDocument.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateProduct_WhenDetailsOnly_ProducesCreatedEventAndSingleUpdatedOutboxEvent()
    {
        await _factory.ResetDatabaseStateAsync();

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
        var createResponse = await client.PostAsJsonAsync("/api/v1/Products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductCreatedResponse>();
        created.Should().NotBeNull();

        var productId = created!.Id;
        var container = _factory.GetCatalogContainer();

        var updateRequest = UpdateProductRequestBuilder.New()
            .WithName("Updated Product Name")
            .WithPriceAmount(149.99m)
            .WithPriceCurrency("EUR")
            .WithCategoryIds()
            .WithRelatedProductIds()
            .WithImages()
            .Build();

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/Products/{productId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var productDocument = await ReadProductDocumentAsync(container, productId);
        productDocument.Id.Should().Be(productId.ToString());
        productDocument.PartitionKey.Should().Be(productId.ToString());
        productDocument.Type.Should().Be("Product");
        productDocument.Sku.Should().Be(sku.ToUpperInvariant());
        productDocument.Name.Should().Be("Updated Product Name");
        productDocument.PriceAmount.Should().Be(149.99m);
        productDocument.PriceCurrency.Should().Be("EUR");
        productDocument.CategoryIds.Should().BeEmpty();
        productDocument.RelatedProductIds.Should().BeEmpty();
        productDocument.Images.Should().BeEmpty();

        var outboxDocuments = await ReadOutboxDocumentsAsync(container, productId.ToString());
        outboxDocuments.Should().HaveCount(2);
        outboxDocuments.Should().OnlyContain(x => x.PartitionKey == productId.ToString());
        outboxDocuments.Count(x => x.EventType == "ProductCreatedEvent").Should().Be(1);
        outboxDocuments.Count(x => x.EventType == "ProductUpdatedEvent").Should().Be(1);

        var createdOutbox = outboxDocuments.Single(x => x.EventType == "ProductCreatedEvent");
        using var createdPayload = JsonDocument.Parse(createdOutbox.Payload);
        createdPayload.RootElement.GetProperty("ProductId").GetString().Should().Be(productId.ToString());
        createdPayload.RootElement.GetProperty("Name").GetString().Should().Be("Original Product");
        createdPayload.RootElement.GetProperty("PriceAmount").GetDecimal().Should().Be(99.99m);
        createdPayload.RootElement.GetProperty("PriceCurrency").GetString().Should().Be("USD");

        var updatedOutbox = outboxDocuments.Single(x => x.EventType == "ProductUpdatedEvent");
        using var updatedPayload = JsonDocument.Parse(updatedOutbox.Payload);
        updatedPayload.RootElement.GetProperty("ProductId").GetString().Should().Be(productId.ToString());
        updatedPayload.RootElement.GetProperty("Name").GetString().Should().Be("Updated Product Name");
        updatedPayload.RootElement.GetProperty("PriceAmount").GetDecimal().Should().Be(149.99m);
        updatedPayload.RootElement.GetProperty("PriceCurrency").GetString().Should().Be("EUR");
        updatedPayload.RootElement.GetProperty("CategoryIds").GetArrayLength().Should().Be(0);
        updatedPayload.RootElement.GetProperty("RelatedProductIds").GetArrayLength().Should().Be(0);
        updatedPayload.RootElement.GetProperty("Images").GetArrayLength().Should().Be(0);
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

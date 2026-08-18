using CLWebStore.Catalog.Domain.Entities;
using CLWebStore.Catalog.Domain.Events;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Mappings;
using CLWebStore.Catalog.UnitTests.Common.Builders;

namespace CLWebStore.Catalog.UnitTests.Infrastructure.Persistence.Cosmos.Mappings;

public class ProductMapperTests
{
    [Fact]
    public void ToDocument_ValidProduct_MapsAllFieldsToProductDocument()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var relatedId = Guid.NewGuid();
        var image = new ProductImage("http://img/1.jpg", "alt", true);

        var product = ProductBuilder.New()
            .WithId(id)
            .WithSku("SKU-123")
            .WithName("Prod Name")
            .WithPrice(19.99m, "USD")
            .WithCategoryIds(categoryId)
            .WithRelatedProductIds(relatedId)
            .WithImages(image)
            .WithVersion("etag-1")
            .Build();

        // Act
        var doc = ProductMapper.ToDocument(product);

        // Assert
        Assert.Equal(id.ToString(), doc.Id);
        Assert.Equal(id.ToString(), doc.PartitionKey);
        Assert.Equal("SKU-123", doc.Sku);
        Assert.Equal("Prod Name", doc.Name);
        Assert.Equal(19.99m, doc.PriceAmount);
        Assert.Equal("USD", doc.PriceCurrency);
        Assert.Single(doc.CategoryIds);
        Assert.Contains(categoryId, doc.CategoryIds);
        Assert.Single(doc.RelatedProductIds);
        Assert.Contains(relatedId, doc.RelatedProductIds);
        Assert.Single(doc.Images);
        var imgDoc = doc.Images.First();
        Assert.Equal(image.Id, imgDoc.Id);
        Assert.Equal(image.Url, imgDoc.Url);
        Assert.Equal(image.AltText, imgDoc.AltText);
        Assert.Equal(image.IsPrimary, imgDoc.IsPrimary);
        Assert.Equal("etag-1", doc.Etag);
    }

    [Fact]
    public void ToDomain_ValidProductDocument_RehydratesProductAggregateCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var relatedId = Guid.NewGuid();
        var imgId = Guid.NewGuid();

        var doc = new ProductDocument
        {
            Id = id.ToString(),
            PartitionKey = id.ToString(),
            Sku = "SKU-XYZ",
            Name = "Mapped Name",
            PriceAmount = 7.5m,
            PriceCurrency = "USD",
            CategoryIds = new List<Guid> { categoryId },
            RelatedProductIds = new List<Guid> { relatedId },
            Images = new List<ProductImageDocument>
            {
                new ProductImageDocument { Id = imgId, Url = "http://x", AltText = "a", IsPrimary = true }
            },
            Etag = "etag-xyz"
        };

        // Act
        var product = ProductMapper.ToDomain(doc);

        // Assert
        Assert.Equal("SKU-XYZ", product.Sku.Value);
        Assert.Equal("Mapped Name", product.Name.Value);
        Assert.Equal(7.5m, product.Price.Amount);
        Assert.Equal("USD", product.Price.Currency);
        Assert.Single(product.CategoryIds);
        Assert.Contains(categoryId, product.CategoryIds);
        Assert.Single(product.RelatedProductIds);
        Assert.Contains(relatedId, product.RelatedProductIds);
        Assert.Single(product.Images);
        var img = product.Images.First();
        Assert.Equal(imgId, img.Id);
        Assert.Equal("http://x", img.Url);
        Assert.Equal("a", img.AltText);
        Assert.True(img.IsPrimary);
        Assert.Equal("etag-xyz", product.Version);
    }

    [Fact]
    public void ToDomain_NullDocument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ProductMapper.ToDomain(null!));
    }

    [Fact]
    public void ToOutboxDocument_ValidDomainEvent_MapsToOutboxDocumentWithJsonPayload()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var evt = new ProductCreatedEvent(
            productId,
            "SKU-1",
            "Name",
            1.23m,
            "USD",
            new List<Guid>(),
            new List<Guid>(),
            new List<ProductImageRecord>(),
            DateTimeOffset.UtcNow);

        // Act
        var outbox = ProductMapper.ToOutboxDocument(evt, productId);

        // Assert
        Assert.Equal(productId.ToString(), outbox.PartitionKey);
        Assert.Equal(nameof(ProductCreatedEvent), outbox.EventType);
        Assert.False(string.IsNullOrWhiteSpace(outbox.Payload));
        // payload should be valid JSON and contain the ProductId
        Assert.Contains(productId.ToString(), outbox.Payload);
    }
}

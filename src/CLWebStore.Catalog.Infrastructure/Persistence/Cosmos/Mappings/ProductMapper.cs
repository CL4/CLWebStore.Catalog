using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Domain.Entities;
using CLWebStore.Catalog.Domain.ValueObjects;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using Newtonsoft.Json;

namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Mappings;

public static class ProductMapper
{
    public static ProductDocument ToDocument(Product product)
    {
        return new ProductDocument
        {
            Id = product.Id.ToString(),
            PartitionKey = product.Id.ToString(), // ProductId is the Partition Key
            Sku = product.Sku.Value,
            Name = product.Name.Value,
            PriceAmount = product.Price.Amount,
            PriceCurrency = product.Price.Currency,
            CategoryIds = product.CategoryIds,
            RelatedProductIds = product.RelatedProductIds,
            Images = [.. product.Images.Select(i => new ProductImageDocument
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary
            })],
            Etag = product.Version
        };
    }

    public static OutboxDocument ToOutboxDocument(object domainEvent, Guid productId)
    {
        return new OutboxDocument
        {
            Id = Guid.NewGuid().ToString(),
            PartitionKey = productId.ToString(), // MUST match Product's Partition Key for TransactionalBatch
            EventType = domainEvent.GetType().Name,
            Payload = JsonConvert.SerializeObject(domainEvent),
            OccurredOn = DateTimeOffset.UtcNow
        };
    }

    public static Product ToDomain(ProductDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        // 1. Reconstruct Value Objects
        var sku = new Sku(document.Sku);
        var name = new ProductName(document.Name);
        var price = new Money(document.PriceAmount, document.PriceCurrency);

        // 2. Reconstruct child Entities
        var images = document.Images.Select(imgDoc =>
        {
            var image = ProductImage.Rehydrate(imgDoc.Id, imgDoc.Url, imgDoc.AltText, imgDoc.IsPrimary);
            return image;
        }).ToList();

        // 3. Rehydrate the Aggregate
        return Product.Rehydrate(
            id: Guid.Parse(document.Id),
            sku: sku,
            name: name,
            price: price,
            categoryIds: document.CategoryIds,
            relatedProductIds: document.RelatedProductIds,
            images: images,
            version: document.Etag // This hooks up Cosmos DB Optimistic Concurrency
        );
    }
}

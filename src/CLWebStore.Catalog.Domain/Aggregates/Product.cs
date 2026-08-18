using CLWebStore.Catalog.Domain.Base;
using CLWebStore.Catalog.Domain.Entities;
using CLWebStore.Catalog.Domain.Events;
using CLWebStore.Catalog.Domain.ValueObjects;

namespace CLWebStore.Catalog.Domain.Aggregates;

public class Product : AggregateRoot
{
    public Sku Sku { get; private set; }
    public ProductName Name { get; private set; }
    public Money Price { get; private set; }

    public List<Guid> CategoryIds { get; private set; } = [];
    public List<Guid> RelatedProductIds { get; private set; } = [];
    public List<ProductImage> Images { get; private set; } = [];

    private Product(Guid id, Sku sku, ProductName name, Money price, string? version)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Price = price;
        Version = version;
    }

    // -------------------------------
    // Factory
    // -------------------------------
    public static Product Create(
        Sku sku,
        ProductName name,
        Money price,
        DateTimeOffset occurredOn)
    {
        var product = new Product(Guid.NewGuid(), sku, name, price, null);

        // Fat Event: Capture the initial state
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            product.Sku.ToString(),
            product.Name.ToString(),
            product.Price.Amount,
            product.Price.Currency,
            [],
            [],
            [],
            occurredOn));

        return product;
    }

    public static Product Rehydrate(
        Guid id, Sku sku, ProductName name, Money price,
        IEnumerable<Guid> categoryIds, IEnumerable<Guid> relatedProductIds,
        IEnumerable<ProductImage> images, string? version)
    {
        var product = new Product(id, sku, name, price, version);
        product.CategoryIds.AddRange(categoryIds);
        product.RelatedProductIds.AddRange(relatedProductIds);
        product.Images.AddRange(images);
        return product;
    }

    // -------------------------------
    // State Snapshot Helper
    // -------------------------------
    // Centralizes the mapping logic so all mutators can easily emit the full state
    private ProductUpdatedEvent CreateUpdatedEvent(DateTimeOffset occurredOn)
    {
        return new ProductUpdatedEvent(
            Id,
            Sku.ToString(),
            Name.ToString(),
            Price.Amount,
            Price.Currency,
            [.. CategoryIds],
            [.. RelatedProductIds],
            Images.Select(i => new ProductImageRecord(i.Id, i.Url, i.AltText, i.IsPrimary)).ToList(),
            occurredOn);
    }

    // -------------------------------
    // Behavior
    // -------------------------------
    public void UpdateDetails(ProductName name, Money price, DateTimeOffset occurredOn)
    {
        Name = name;
        Price = price;
        AddDomainEvent(CreateUpdatedEvent(occurredOn));
    }

    public void AddCategory(Guid categoryId, DateTimeOffset occurredOn)
    {
        if (CategoryIds.Contains(categoryId)) return;

        CategoryIds.Add(categoryId);
        AddDomainEvent(CreateUpdatedEvent(occurredOn));
    }

    public void RemoveCategory(Guid categoryId, DateTimeOffset occurredOn)
    {
        if (!CategoryIds.Contains(categoryId)) return;

        CategoryIds.Remove(categoryId);
        AddDomainEvent(CreateUpdatedEvent(occurredOn));
    }

    public void AddRelatedProduct(Guid productId, DateTimeOffset occurredOn)
    {
        if (productId == Id) throw new DomainException("Product cannot relate to itself");
        if (RelatedProductIds.Contains(productId)) return;

        RelatedProductIds.Add(productId);
        AddDomainEvent(CreateUpdatedEvent(occurredOn));
    }

    public void RemoveRelatedProduct(Guid productId, DateTimeOffset occurredOn)
    {
        if (!RelatedProductIds.Contains(productId)) return;

        RelatedProductIds.Remove(productId);
        AddDomainEvent(CreateUpdatedEvent(occurredOn));
    }

    public void AddImage(string url, string altText, bool isPrimary, DateTimeOffset occurredOn)
    {
        Images.Add(new ProductImage(url, altText, isPrimary));
        AddDomainEvent(CreateUpdatedEvent(occurredOn));
    }

    public void UpdateImage(Guid imageId, string url, string altText, bool isPrimary, DateTimeOffset occurredOn)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            image.Update(url, altText, isPrimary);
            AddDomainEvent(CreateUpdatedEvent(occurredOn));
        }
    }

    public void RemoveImage(Guid imageId, DateTimeOffset occurredOn)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            Images.Remove(image);
            AddDomainEvent(CreateUpdatedEvent(occurredOn));
        }
    }
}
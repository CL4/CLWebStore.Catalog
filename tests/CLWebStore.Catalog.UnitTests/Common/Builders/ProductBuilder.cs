using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Domain.Entities;
using CLWebStore.Catalog.Domain.ValueObjects;

namespace CLWebStore.Catalog.UnitTests.Common.Builders;

public class ProductBuilder
{
    private Guid? _id;
    private Sku _sku = new Sku("SKU-001");
    private ProductName _name = new ProductName("Default Product");
    private Money _price = new Money(9.99m, "USD");
    private List<Guid> _categoryIds = new List<Guid>();
    private List<Guid> _relatedProductIds = new List<Guid>();
    private List<ProductImage> _images = new List<ProductImage>();
    private string? _version;

    public ProductBuilder()
    {
    }

    public static ProductBuilder New() => new ProductBuilder();

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithSku(string sku)
    {
        _sku = new Sku(sku);
        return this;
    }

    public ProductBuilder WithSku(Sku sku)
    {
        _sku = sku ?? throw new ArgumentNullException(nameof(sku));
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = new ProductName(name);
        return this;
    }

    public ProductBuilder WithName(ProductName name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    public ProductBuilder WithPrice(decimal amount, string currency = "USD")
    {
        _price = new Money(amount, currency);
        return this;
    }

    public ProductBuilder WithPrice(Money price)
    {
        _price = price ?? throw new ArgumentNullException(nameof(price));
        return this;
    }

    public ProductBuilder WithImages(params ProductImage[] images)
    {
        _images = images?.ToList() ?? new List<ProductImage>();
        return this;
    }

    public ProductBuilder WithCategoryIds(params Guid[] ids)
    {
        _categoryIds = ids?.ToList() ?? new List<Guid>();
        return this;
    }

    public ProductBuilder WithRelatedProductIds(params Guid[] ids)
    {
        _relatedProductIds = ids?.ToList() ?? new List<Guid>();
        return this;
    }

    public ProductBuilder WithVersion(string? version)
    {
        _version = version;
        return this;
    }

    public Product Build()
    {
        if (_id.HasValue)
        {
            // Rehydrate with provided id and collections
            return Product.Rehydrate(
                _id.Value,
                _sku,
                _name,
                _price,
                _categoryIds,
                _relatedProductIds,
                _images,
                _version);
        }

        // Create will generate a new Id and emit the created event
        return Product.Create(_sku, _name, _price, DateTimeOffset.UtcNow);
    }
}

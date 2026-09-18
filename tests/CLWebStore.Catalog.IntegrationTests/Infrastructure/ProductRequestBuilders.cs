using CLWebStore.Catalog.API.Contracts.V1.Requests;

namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

public sealed class CreateProductRequestBuilder
{
    private string _sku = "TEST-SKU-001";
    private string _name = "Test Product";
    private decimal _priceAmount = 99.99m;
    private string _priceCurrency = "USD";
    private List<Guid>? _categoryIds = [Guid.NewGuid()];
    private List<Guid>? _relatedProductIds = [];
    private List<ProductImageRequest>? _images =
    [
        new ProductImageRequest("https://example.com/images/test-product.jpg", "Test product image", true)
    ];

    public static CreateProductRequestBuilder New() => new();

    public CreateProductRequestBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public CreateProductRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateProductRequestBuilder WithPriceAmount(decimal priceAmount)
    {
        _priceAmount = priceAmount;
        return this;
    }

    public CreateProductRequestBuilder WithPriceCurrency(string priceCurrency)
    {
        _priceCurrency = priceCurrency;
        return this;
    }

    public CreateProductRequestBuilder WithCategoryIds(params Guid[] categoryIds)
    {
        _categoryIds = categoryIds.ToList();
        return this;
    }

    public CreateProductRequestBuilder WithRelatedProductIds(params Guid[] relatedProductIds)
    {
        _relatedProductIds = relatedProductIds.ToList();
        return this;
    }

    public CreateProductRequestBuilder WithImages(params ProductImageRequest[] images)
    {
        _images = images.ToList();
        return this;
    }

    public CreateProductRequest Build() => new(
        _sku,
        _name,
        _priceAmount,
        _priceCurrency,
        _categoryIds,
        _relatedProductIds,
        _images);
}

public sealed class UpdateProductRequestBuilder
{
    private string _name = "Updated Test Product";
    private decimal _priceAmount = 149.99m;
    private string _priceCurrency = "USD";
    private List<Guid>? _categoryIds = [Guid.NewGuid()];
    private List<Guid>? _relatedProductIds = [];
    private List<UpdateProductImageRequest>? _images =
    [
        new UpdateProductImageRequest(Guid.NewGuid(), "https://example.com/images/updated-product.jpg", "Updated product image", true)
    ];

    public static UpdateProductRequestBuilder New() => new();

    public UpdateProductRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UpdateProductRequestBuilder WithPriceAmount(decimal priceAmount)
    {
        _priceAmount = priceAmount;
        return this;
    }

    public UpdateProductRequestBuilder WithPriceCurrency(string priceCurrency)
    {
        _priceCurrency = priceCurrency;
        return this;
    }

    public UpdateProductRequestBuilder WithCategoryIds(params Guid[] categoryIds)
    {
        _categoryIds = categoryIds.ToList();
        return this;
    }

    public UpdateProductRequestBuilder WithRelatedProductIds(params Guid[] relatedProductIds)
    {
        _relatedProductIds = relatedProductIds.ToList();
        return this;
    }

    public UpdateProductRequestBuilder WithImages(params UpdateProductImageRequest[] images)
    {
        _images = images.ToList();
        return this;
    }

    public UpdateProductRequest Build() => new(
        _name,
        _priceAmount,
        _priceCurrency,
        _categoryIds,
        _relatedProductIds,
        _images);
}

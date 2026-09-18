namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

public sealed record ProductFixture(
    Guid Id,
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    Guid[] CategoryIds,
    Guid[] RelatedProductIds,
    List<ProductImageFixture> Images
);

public sealed record ProductImageFixture(
    string Url,
    string AltText,
    bool IsPrimary
);

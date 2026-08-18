namespace CLWebStore.Catalog.API.Contracts.V1.Requests;

public record CreateProductRequest(
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    List<Guid>? CategoryIds,
    List<Guid>? RelatedProductIds,
    List<ProductImageRequest>? Images
);

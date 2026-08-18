namespace CLWebStore.Catalog.API.Contracts.V1.Requests;

public record UpdateProductRequest(
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    List<Guid>? CategoryIds,
    List<Guid>? RelatedProductIds,
    List<UpdateProductImageRequest>? Images
);

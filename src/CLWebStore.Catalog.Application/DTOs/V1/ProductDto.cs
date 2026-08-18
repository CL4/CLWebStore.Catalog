namespace CLWebStore.Catalog.Application.DTOs.V1;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public string? Version { get; init; }

    public List<Guid> CategoryIds { get; init; } = [];
    public List<Guid> RelatedProductIds { get; init; } = [];
    public List<ProductImageDto> Images { get; init; } = [];
}

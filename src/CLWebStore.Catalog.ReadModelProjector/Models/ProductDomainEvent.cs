namespace CLWebStore.Catalog.ReadModelProjector.Models;

public sealed record ProductDomainEvent
{
    public Guid ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal PriceAmount { get; init; }

    public string PriceCurrency { get; init; } = string.Empty;

    public IReadOnlyCollection<Guid>? CategoryIds { get; init; } = [];

    public IReadOnlyCollection<Guid>? RelatedProductIds { get; init; } = [];

    public IReadOnlyCollection<ProductImageRecord>? Images { get; init; } = [];

    public DateTimeOffset OccurredOn { get; init; }
}

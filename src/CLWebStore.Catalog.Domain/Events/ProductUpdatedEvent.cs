using CLWebStore.Catalog.Domain.Base;

namespace CLWebStore.Catalog.Domain.Events;

public record ProductUpdatedEvent(
    Guid ProductId,
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    List<Guid> CategoryIds,
    List<Guid> RelatedProductIds,
    List<ProductImageRecord> Images,
    DateTimeOffset OccurredOn) : IDomainEvent;

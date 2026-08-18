namespace CLWebStore.Catalog.Domain.Events;

public record ProductImageRecord(Guid Id, string Url, string AltText, bool IsPrimary);

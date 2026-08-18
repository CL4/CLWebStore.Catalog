namespace CLWebStore.Catalog.ReadModelProjector.Models;

public sealed record ProductImageRecord(
    Guid Id,
    string Url,
    string AltText,
    bool IsPrimary);

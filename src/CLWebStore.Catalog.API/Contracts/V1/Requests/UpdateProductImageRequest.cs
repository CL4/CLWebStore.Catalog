namespace CLWebStore.Catalog.API.Contracts.V1.Requests;

public record UpdateProductImageRequest(
    Guid? Id, // Nullable, as new images won't have an ID yet
    string Url,
    string AltText,
    bool IsPrimary
);

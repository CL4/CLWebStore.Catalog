namespace CLWebStore.Catalog.API.Contracts.V1.Requests;

public record ProductImageRequest(
    string Url,
    string AltText,
    bool IsPrimary
);

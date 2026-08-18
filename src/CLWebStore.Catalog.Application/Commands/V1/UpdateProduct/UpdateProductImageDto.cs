namespace CLWebStore.Catalog.Application.DTOs.V1;

public record UpdateProductImageDto(
    Guid? Id,
    string Url,
    string AltText,
    bool IsPrimary
);

namespace CLWebStore.Catalog.Application.DTOs.V1;

public record CreateProductImageDto(
    string Url,
    string AltText,
    bool IsPrimary
);

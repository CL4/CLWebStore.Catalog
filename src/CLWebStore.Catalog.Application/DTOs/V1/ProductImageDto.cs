namespace CLWebStore.Catalog.Application.DTOs.V1;

public record ProductImageDto(Guid Id, string Url, string AltText, bool IsPrimary);

using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;

public record UpdateProductCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public List<Guid>? CategoryIds { get; init; }
    public List<Guid>? RelatedProductIds { get; init; }
    public List<UpdateProductImageDto>? Images { get; init; }
}

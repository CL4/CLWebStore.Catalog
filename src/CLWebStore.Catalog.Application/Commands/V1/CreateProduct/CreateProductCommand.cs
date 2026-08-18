using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Commands.V1.CreateProduct;

public record CreateProductCommand(
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    List<Guid>? CategoryIds,
    List<Guid>? RelatedProductIds,
    List<CreateProductImageDto>? Images) : IRequest<Guid>;

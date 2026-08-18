using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetProductsBySku;

public record GetProductsBySkuQuery(IEnumerable<string> Skus) : IRequest<IEnumerable<ProductDto>>;

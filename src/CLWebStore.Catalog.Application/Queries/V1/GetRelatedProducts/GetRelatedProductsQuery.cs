using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetRelatedProducts;

public record GetRelatedProductsQuery(Guid ProductId) : IRequest<IEnumerable<ProductDto>>;

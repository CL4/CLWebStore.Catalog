using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetProductsByCategory;

public record GetProductsByCategoryQuery(Guid CategoryId) : IRequest<IEnumerable<ProductDto>>;

using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.SearchProducts;

public record SearchProductsQuery(string Query, int Limit = 20) : IRequest<IEnumerable<ProductDto>>;

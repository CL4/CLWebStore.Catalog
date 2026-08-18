using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.SearchProducts;

public class SearchProductsHandler(CLWebStore.Catalog.Application.Abstractions.V1.IProductQueryService queryService)
    : IRequestHandler<SearchProductsQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return await queryService.SearchProductsAsync(request.Query, request.Limit, cancellationToken)
               ?? throw new NotFoundException(nameof(Product), request.Query);
    }
}

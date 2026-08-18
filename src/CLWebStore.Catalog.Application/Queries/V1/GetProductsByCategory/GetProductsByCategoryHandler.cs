using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetProductsByCategory;

public class GetProductsByCategoryHandler(CLWebStore.Catalog.Application.Abstractions.V1.IProductQueryService queryService)
    : IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        return await queryService.GetProductsByCategoryAsync(request.CategoryId, cancellationToken)
               ?? throw new NotFoundException(nameof(Product), request.CategoryId);
    }
}

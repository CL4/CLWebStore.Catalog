using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetRelatedProducts;

public class GetRelatedProductsHandler(CLWebStore.Catalog.Application.Abstractions.V1.IProductQueryService queryService)
    : IRequestHandler<GetRelatedProductsQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken)
    {
        return await queryService.GetRelatedProductsAsync(request.ProductId, cancellationToken)
               ?? throw new NotFoundException(nameof(Product), request.ProductId);
    }
}

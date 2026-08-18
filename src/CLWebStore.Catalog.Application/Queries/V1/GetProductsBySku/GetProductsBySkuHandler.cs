using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetProductsBySku;

public class GetProductsBySkuHandler(CLWebStore.Catalog.Application.Abstractions.V1.IProductQueryService queryService)
    : IRequestHandler<GetProductsBySkuQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetProductsBySkuQuery request, CancellationToken cancellationToken)
    {
        return await queryService.GetProductsBySkuAsync(request.Skus, cancellationToken)
               ?? throw new NotFoundException(nameof(Product), request.Skus);
    }
}

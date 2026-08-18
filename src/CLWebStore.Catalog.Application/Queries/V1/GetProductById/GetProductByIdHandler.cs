using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetProductById;

public class GetProductByIdHandler(IProductQueryService queryService)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await queryService.GetProductByIdAsync(request.Id, cancellationToken)
               ?? throw new NotFoundException(nameof(Product), request.Id);
    }
}

using CLWebStore.Catalog.Application.DTOs.V1;
using MediatR;

namespace CLWebStore.Catalog.Application.Queries.V1.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

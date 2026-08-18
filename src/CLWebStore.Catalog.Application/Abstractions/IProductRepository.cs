using CLWebStore.Catalog.Domain.Aggregates;

namespace CLWebStore.Catalog.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        Product product,
        CancellationToken cancellationToken = default);
}

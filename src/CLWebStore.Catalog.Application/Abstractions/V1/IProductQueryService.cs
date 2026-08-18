using CLWebStore.Catalog.Application.DTOs.V1;

namespace CLWebStore.Catalog.Application.Abstractions.V1;

public interface IProductQueryService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductDto>> GetProductsBySkuAsync(IEnumerable<string> skus, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductDto>> SearchProductsAsync(string query, int limit = 20, CancellationToken cancellationToken = default);
}

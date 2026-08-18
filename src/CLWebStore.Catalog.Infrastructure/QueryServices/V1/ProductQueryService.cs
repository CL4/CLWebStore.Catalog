using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Infrastructure.Sql.V1.Products;
using Dapper;
using System.Data;

namespace CLWebStore.Catalog.Infrastructure.QueryServices.V1;

public class ProductQueryService : IProductQueryService
{
    private readonly IDbConnection _connection;

    public ProductQueryService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: GetProductByIdQuery.Sql,
            parameters: new GetProductByIdQuery.Parameters(id),
            cancellationToken: cancellationToken);

        return await _connection.QuerySingleOrDefaultAsync<ProductDto>(command);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsBySkuAsync(IEnumerable<string> skus, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: GetProductsBySkuQuery.Sql,
            parameters: new GetProductsBySkuQuery.Parameters(skus),
            cancellationToken: cancellationToken);

        var result = await _connection.QueryAsync<ProductDto>(command);
        return result;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: GetProductsByCategoryQuery.Sql,
            parameters: new GetProductsByCategoryQuery.Parameters(categoryId),
            cancellationToken: cancellationToken);

        var result = await _connection.QueryAsync<ProductDto>(command);
        return result;
    }

    public async Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: GetRelatedProductsQuery.Sql,
            parameters: new GetRelatedProductsQuery.Parameters(productId),
            cancellationToken: cancellationToken);

        var result = await _connection.QueryAsync<ProductDto>(command);
        return result;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string query, int limit = 20, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: SearchProductsQuery.Sql,
            parameters: new SearchProductsQuery.Parameters(query, limit),
            cancellationToken: cancellationToken);

        var result = await _connection.QueryAsync<ProductDto>(command);
        return result;
    }
}

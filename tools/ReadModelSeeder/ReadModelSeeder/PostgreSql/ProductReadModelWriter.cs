using Dapper;
using Npgsql;
using ReadModelSeeder.Models;
using System.Text.Json;

namespace ReadModelSeeder.PostgreSql;

public sealed partial class ProductReadModelWriter : IProductReadModelWriter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.General);

    private readonly NpgsqlDataSource _dataSource;

    public ProductReadModelWriter(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task UpsertAsync(ProductDocument product, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(product.Id, out Guid productId))
        {
            throw new InvalidOperationException(
                $"Product document id '{product.Id}' cannot be migrated because it is not a valid UUID.");
        }

        var parameters = new UpsertReadSchemaSql.Parameters(
            productId,
            product.Sku,
            product.Name,
            product.PriceAmount,
            product.PriceCurrency,
            product.Etag,
            product.CategoryIds.ToArray(),
            product.RelatedProductIds.ToArray(),
            new JsonbParameter(JsonSerializer.Serialize(product.Images, JsonSerializerOptions))
        );

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: UpsertReadSchemaSql.Sql,
            parameters: parameters,
            cancellationToken: cancellationToken
        );

        await connection.ExecuteAsync(command);
    }
}

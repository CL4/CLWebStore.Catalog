
using CLWebStore.Catalog.ReadModelProjector.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Text.Json;
namespace CLWebStore.Catalog.ReadModelProjector.Projection;

public sealed class ProductProjectionService : IProductProjectionService
{
    private const string UpsertSql = """
        INSERT INTO read_schema.Products (
            id,
            sku,
            name,
            price_amount,
            price_currency,
            category_ids,
            related_product_ids,
            images
        )
        VALUES (
            @Id,
            @Sku,
            @Name,
            @PriceAmount,
            @PriceCurrency,
            @CategoryIds,
            @RelatedProductIds,
            @Images
        )
        ON CONFLICT (id)
        DO UPDATE SET
            sku = EXCLUDED.sku,
            name = EXCLUDED.name,
            price_amount = EXCLUDED.price_amount,
            price_currency = EXCLUDED.price_currency,
            category_ids = EXCLUDED.category_ids,
            related_product_ids = EXCLUDED.related_product_ids,
            images = EXCLUDED.images
        """;

    private readonly IConnectionFactory _connectionFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<ProductProjectionService> _logger;

    public ProductProjectionService(
        IConnectionFactory connectionFactory,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<ProductProjectionService> logger)
    {
        _connectionFactory = connectionFactory;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
    }

    public async Task UpsertAsync(ProductDomainEvent productEvent, CancellationToken cancellationToken)
    {
        var imagesJson = JsonSerializer.Serialize(productEvent.Images ?? [], _jsonSerializerOptions);
        var parameters = new
        {
            Id = productEvent.ProductId,
            productEvent.Sku,
            productEvent.Name,
            productEvent.PriceAmount,
            productEvent.PriceCurrency,
            CategoryIds = productEvent.CategoryIds?.ToArray() ?? [],
            RelatedProductIds = productEvent.RelatedProductIds?.ToArray() ?? [],
            Images = new JsonbParameter(imagesJson)
        };

        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                commandText: UpsertSql,
                parameters: parameters,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);

            _logger.LogInformation(
                "Successfully upserted product read model for product {ProductId}.",
                productEvent.ProductId);
        }
        catch (Exception ex) when (ex is DbException or TimeoutException or InvalidOperationException)
        {
            _logger.LogError(
                ex,
                "Retryable infrastructure failure while upserting product read model for product {ProductId}.",
                productEvent.ProductId);

            throw;
        }
    }
}
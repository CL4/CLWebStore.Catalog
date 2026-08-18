using System.Diagnostics;
using ReadModelSeeder.Cosmos;
using ReadModelSeeder.PostgreSql;
using Microsoft.Extensions.Logging;

namespace ReadModelSeeder.Services;

public sealed class ProductMigrationService
{
    private readonly ICosmosProductReader _cosmosProductReader;
    private readonly IProductReadModelWriter _productReadModelWriter;
    private readonly ILogger<ProductMigrationService> _logger;

    public ProductMigrationService(
        ICosmosProductReader cosmosProductReader,
        IProductReadModelWriter productReadModelWriter,
        ILogger<ProductMigrationService> logger)
    {
        _cosmosProductReader = cosmosProductReader;
        _productReadModelWriter = productReadModelWriter;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var successfulMigrations = 0;
        var failedMigrations = 0;

        IReadOnlyList<Models.ProductDocument> products =
            await _cosmosProductReader.GetProductsAsync(cancellationToken);

        _logger.LogInformation("Found {ProductCount} Product documents to migrate.", products.Count);

        foreach (Models.ProductDocument product in products)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _productReadModelWriter.UpsertAsync(product, cancellationToken);
                successfulMigrations++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedMigrations++;

                _logger.LogError(
                    exception,
                    "Failed to migrate Product document {ProductId} with SKU {Sku}.",
                    product.Id,
                    product.Sku);
            }
        }

        stopwatch.Stop();

        _logger.LogInformation(
            "Product migration completed. Total products discovered: {TotalProducts}. Successful migrations: {SuccessfulMigrations}. Failed migrations: {FailedMigrations}. Total execution time: {Elapsed}.",
            products.Count,
            successfulMigrations,
            failedMigrations,
            stopwatch.Elapsed);
    }
}

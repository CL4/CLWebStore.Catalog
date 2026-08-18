using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReadModelSeeder.Models;
using ReadModelSeeder.Settings;

namespace ReadModelSeeder.Cosmos;

public sealed class CosmosProductReader : ICosmosProductReader
{
    private readonly Container _container;
    private readonly ILogger<CosmosProductReader> _logger;

    public CosmosProductReader(
        CosmosClient cosmosClient,
        IOptions<MigrationSettings> settings,
        ILogger<CosmosProductReader> logger)
    {
        _logger = logger;

        var migrationSettings = settings.Value;
        _container = cosmosClient.GetContainer(
            migrationSettings.CosmosDbDatabaseName,
            migrationSettings.CosmosDbContainerName);
    }

    public async Task<IReadOnlyList<ProductDocument>> GetProductsAsync(CancellationToken cancellationToken)
    {
        const string queryText = "SELECT * FROM c WHERE IS_DEFINED(c.type) AND c.type = @type";

        var products = new List<ProductDocument>();
        var query = new QueryDefinition(queryText).WithParameter("@type", "Product");

        using FeedIterator<ProductDocument> feedIterator = _container.GetItemQueryIterator<ProductDocument>(
            query,
            requestOptions: new QueryRequestOptions
            {
                MaxItemCount = 100
            });

        while (feedIterator.HasMoreResults)
        {
            FeedResponse<ProductDocument> response = await feedIterator.ReadNextAsync(cancellationToken);
            products.AddRange(response);

            _logger.LogDebug(
                "Read {ProductCount} Product documents from Cosmos DB page with request charge {RequestCharge}.",
                response.Count,
                response.RequestCharge);
        }

        return products;
    }
}

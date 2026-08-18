using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;

public class CosmosClientFactory : ICosmosClientFactory
{
    private readonly CosmosClient _client;
    private readonly CosmosSettings _settings;

    public CosmosClientFactory(IOptions<CosmosSettings> settings)
    {
        _settings = settings.Value;

        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        _client = new CosmosClient(_settings.PrimaryConnectionString, options);
    }

    public Container GetCatalogContainer()
    {
        return _client.GetContainer(_settings.DatabaseName, _settings.ContainerName);
    }

    public CosmosClient GetClient()
    {
        return _client;
    }
}

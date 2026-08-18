using Microsoft.Azure.Cosmos;

namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;

public interface ICosmosClientFactory
{
    Container GetCatalogContainer();
    CosmosClient GetClient();
}
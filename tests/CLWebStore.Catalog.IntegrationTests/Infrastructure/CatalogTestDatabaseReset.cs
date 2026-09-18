using Microsoft.Azure.Cosmos;
using Npgsql;
using Respawn;

namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

public static class CatalogTestDatabaseReset
{
    public static async Task ResetAsync(CatalogWebApplicationFactory factory)
    {
        await ResetPostgresAsync(factory.PostgresConnectionString);
        await DeleteCosmosDocumentsAsync(factory.GetCatalogContainer());
    }

    private static async Task ResetPostgresAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["read_schema"],
            WithReseed = true,
        });

        await respawner.ResetAsync(connection);
    }

    private static async Task DeleteCosmosDocumentsAsync(Container container)
    {
        var iterator = container.GetItemQueryIterator<dynamic>(
            new QueryDefinition("SELECT c.id, c.partitionKey FROM c"));

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();

            foreach (var item in page)
            {
                var id = item?.id?.ToString();
                var partitionKey = item?.partitionKey?.ToString();

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(partitionKey))
                {
                    continue;
                }

                await container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));
            }
        }
    }
}

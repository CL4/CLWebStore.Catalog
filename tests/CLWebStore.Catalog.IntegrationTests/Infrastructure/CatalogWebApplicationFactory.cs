using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;
using Dapper;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.CosmosDb;
using Testcontainers.PostgreSql;

namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

public sealed class CatalogWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private TestConfiguredCosmosClientFactory? _cosmosClientFactory;

    private const string PostgresDatabaseName = "catalog_testdb";
    private const string PostgresUsername = "cataloguser";
    private const string PostgresPassword = "catalogpass!";
    private const string PostgresSchemaSql = """
        CREATE SCHEMA IF NOT EXISTS read_schema;

        CREATE TABLE IF NOT EXISTS read_schema.Products (
            Id UUID PRIMARY KEY,
            Sku VARCHAR(50) NOT NULL UNIQUE,
            Name VARCHAR(255) NOT NULL,
            PriceAmount NUMERIC(18, 4) NOT NULL,
            PriceCurrency VARCHAR(3) NOT NULL,
            Version VARCHAR(50) NULL,
            CategoryIds UUID[] NOT NULL DEFAULT '{}',
            RelatedProductIds UUID[] NOT NULL DEFAULT '{}',
            Images JSONB NOT NULL DEFAULT '[]'
        );

        CREATE INDEX IF NOT EXISTS IX_Products_Sku
            ON read_schema.Products (Sku);

        CREATE INDEX IF NOT EXISTS IX_Products_CategoryIds
            ON read_schema.Products USING GIN (CategoryIds);
        """;

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase(PostgresDatabaseName)
        .WithUsername(PostgresUsername)
        .WithPassword(PostgresPassword)
        .Build();

    private readonly CosmosDbContainer _cosmosContainer = new CosmosDbBuilder("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview")
        .WithWaitStrategy(
            Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(
                    request => request
                        .ForPath("/")
                        .ForPort(8081)
                        .UsingTls(false),
                    waitStrategy => waitStrategy.WithTimeout(TimeSpan.FromMinutes(3))))
        .Build();

    public string PostgresConnectionString => _postgresContainer.GetConnectionString();
    public string CosmosConnectionString => _cosmosContainer.GetConnectionString();
    public const string CosmosDatabaseName = "CatalogDb";
    public const string CosmosContainerName = "Catalog";

    private TestConfiguredCosmosClientFactory CosmosClientFactory =>
        _cosmosClientFactory ??= new TestConfiguredCosmosClientFactory(CosmosConnectionString);

    public Container GetCatalogContainer()
    {
        return CosmosClientFactory.GetCatalogContainer();
    }

    public async Task ResetDatabaseStateAsync()
    {
        await CatalogTestDatabaseReset.ResetAsync(this);
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _cosmosContainer.StartAsync();

        _cosmosClientFactory = new TestConfiguredCosmosClientFactory(CosmosConnectionString);

        await InitializePostgresSchemaAsync();
        await InitializeCosmosSchemaAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _cosmosContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        Environment.SetEnvironmentVariable("KeyVault__VaultUri", string.Empty);
        Environment.SetEnvironmentVariable("Observability__EnableNewRelicExport", "false");
        Environment.SetEnvironmentVariable("Observability__NewRelicApiKey", string.Empty);
        Environment.SetEnvironmentVariable("Observability__NewRelicOtlpEndpoint", string.Empty);
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSQL", PostgresConnectionString);
        Environment.SetEnvironmentVariable("CosmosSettings__PrimaryConnectionString", CosmosConnectionString);
        Environment.SetEnvironmentVariable("CosmosSettings__DatabaseName", CosmosDatabaseName);
        Environment.SetEnvironmentVariable("CosmosSettings__ContainerName", CosmosContainerName);

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["KeyVault:VaultUri"] = string.Empty,
                ["Observability:EnableNewRelicExport"] = "false",
                ["Observability:NewRelicApiKey"] = string.Empty,
                ["Observability:NewRelicOtlpEndpoint"] = string.Empty,
                ["ConnectionStrings:PostgreSQL"] = PostgresConnectionString,
                ["CosmosSettings:PrimaryConnectionString"] = CosmosConnectionString,
                ["CosmosSettings:DatabaseName"] = CosmosDatabaseName,
                ["CosmosSettings:ContainerName"] = CosmosContainerName,
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<ICosmosClientFactory>(_ => CosmosClientFactory);
        });
    }

    private async Task InitializePostgresSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(PostgresSchemaSql);
    }

    private async Task InitializeCosmosSchemaAsync()
    {
        using var client = CreateCosmosClient(CosmosConnectionString);

        DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync(CosmosDatabaseName);
        Database database = databaseResponse.Database;
        ContainerProperties containerProperties = new(CosmosContainerName, "/partitionKey");

        await database.CreateContainerIfNotExistsAsync(containerProperties);
    }

    private static CosmosClient CreateCosmosClient(string connectionString)
    {
        var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        });

        var options = new CosmosClientOptions
        {
            HttpClientFactory = () => httpClient,
            ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Gateway,
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            },
        };

        return new CosmosClient(connectionString, options);
    }

    private sealed class TestConfiguredCosmosClientFactory : ICosmosClientFactory
    {
        private readonly CosmosClient _client;

        public TestConfiguredCosmosClientFactory(string connectionString)
        {
            _client = CreateCosmosClient(connectionString);
        }

        public Container GetCatalogContainer()
        {
            return _client.GetContainer(CosmosDatabaseName, CosmosContainerName);
        }

        public CosmosClient GetClient()
        {
            return _client;
        }
    }
}

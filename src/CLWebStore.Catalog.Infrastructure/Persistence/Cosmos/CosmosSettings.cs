namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;

public class CosmosSettings
{
    public string PrimaryConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

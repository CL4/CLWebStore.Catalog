namespace ReadModelSeeder.Settings;

public sealed class MigrationSettings
{
    public const string SectionName = "MigrationSettings";

    public string CosmosDbDatabaseName { get; set; } = string.Empty;

    public string CosmosDbContainerName { get; set; } = string.Empty;
}

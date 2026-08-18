namespace CLWebStore.Catalog.ReadModelProjector.Configuration;

internal sealed record ProjectorOptions
{
    public string ServiceBusConnection { get; init; } = string.Empty;

    public string ServiceBusSubscriptionName { get; init; } = string.Empty;

    public string PostgresConnectionString { get; init; } = string.Empty;
}

namespace CLWebStore.Catalog.ReadModelProjector.Configuration;

internal sealed record NewRelicOptions
{
    public const string SectionName = "NewRelic";

    public string OtlpEndpoint { get; init; } = string.Empty;

    public string LicenseKey { get; init; } = string.Empty;
}

using Newtonsoft.Json;

namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;

public abstract class BaseDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = string.Empty;

    [JsonProperty("type")]
    public abstract string Type
    {
        get;
    }

    [JsonProperty("_etag")]
    public string? Etag
    {
        get; set;
    }
}

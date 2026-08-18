using System.Text.Json.Serialization;

namespace CLWebStore.Catalog.OutboxProcessor.Models;

public class OutboxMessage
{
    [JsonPropertyName("id")]
    public string Id
    {
        get; set;
    }

    [JsonPropertyName("type")]
    public string Type
    {
        get; set;
    }

    [JsonPropertyName("payload")]
    public string Payload
    {
        get; set;
    }

    // Add any other relevant properties from your Cosmos DB Outbox documents
}

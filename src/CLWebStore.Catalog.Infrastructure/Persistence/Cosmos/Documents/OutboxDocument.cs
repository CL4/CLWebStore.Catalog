namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;

public class OutboxDocument : BaseDocument
{
    public override string Type => "OutboxEvent";

    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredOn
    {
        get; set;
    }
}

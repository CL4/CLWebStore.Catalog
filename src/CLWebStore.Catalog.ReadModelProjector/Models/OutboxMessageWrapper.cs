namespace CLWebStore.Catalog.ReadModelProjector.Models;

/// <summary>
/// Represents the outbox envelope received from Azure Service Bus.
/// </summary>
public sealed record OutboxMessageWrapper
{
    /// <summary>
    /// Gets the unique outbox message identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the domain event type stored in the payload.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the serialized domain event payload.
    /// </summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>
    /// Gets the time at which the outbox message occurred.
    /// </summary>
    public DateTimeOffset OccurredOn { get; init; }
}

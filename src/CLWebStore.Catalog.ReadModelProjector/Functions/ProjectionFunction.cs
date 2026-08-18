using CLWebStore.Catalog.ReadModelProjector.Dispatching;
using CLWebStore.Catalog.ReadModelProjector.Models;
using CLWebStore.Catalog.ReadModelProjector.Observability;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CLWebStore.Catalog.ReadModelProjector.Functions;

/// <summary>
/// Azure Function entry point that receives Catalog outbox messages from Azure Service Bus.
/// </summary>
public sealed class ProjectionFunction
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<ProjectionFunction> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionFunction"/> class.
    /// </summary>
    public ProjectionFunction(
        IEventDispatcher eventDispatcher,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<ProjectionFunction> logger)
    {
        _eventDispatcher = eventDispatcher;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
    }

    /// <summary>
    /// Processes a single product outbox message.
    /// </summary>
    /// <param name="message">The Service Bus message body.</param>
    /// <param name="context">The Azure Functions execution context.</param>
    /// <param name="cancellationToken">A token that cancels the in-flight projection.</param>
    [Function(nameof(ProjectionFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "catalog-outbox-changes",
            "%ServiceBusSubscriptionName%",
            Connection = "ServiceBusConnection")]
        BinaryData message,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        var metadata = ServiceBusMessageMetadata.From(context);

        using var activity = ReadModelProjectorDiagnostics.ActivitySource.StartActivity(
            "Process catalog outbox message",
            System.Diagnostics.ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "azure_service_bus");
        activity?.SetTag("messaging.destination.name", "catalog-outbox-changes");
        activity?.SetTag("messaging.message.id", metadata.MessageId);

        _logger.LogInformation("Projection function started for message {MessageId}.", metadata.MessageId);

        OutboxMessageWrapper wrapper;

        try
        {
            wrapper = JsonSerializer.Deserialize<OutboxMessageWrapper>(message, _jsonSerializerOptions)
                ?? throw new JsonException("Outbox message wrapper deserialized to null.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize outbox message wrapper. MessageId: {MessageId}, CorrelationId: {CorrelationId}, DeliveryCount: {DeliveryCount}, EnqueuedTime: {EnqueuedTime}.",
                metadata.MessageId,
                metadata.CorrelationId,
                metadata.DeliveryCount,
                metadata.EnqueuedTime);

            throw;
        }

        activity?.SetTag("catalog.event_type", wrapper.EventType);

        _logger.LogInformation(
            "Service Bus message received. MessageId: {MessageId}, CorrelationId: {CorrelationId}, DeliveryCount: {DeliveryCount}, EnqueuedTime: {EnqueuedTime}, EventType: {EventType}.",
            metadata.MessageId,
            metadata.CorrelationId,
            metadata.DeliveryCount,
            metadata.EnqueuedTime,
            wrapper.EventType);

        await _eventDispatcher.DispatchAsync(wrapper, cancellationToken);
    }
}

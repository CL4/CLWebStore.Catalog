using CLWebStore.Catalog.OutboxProcessor.Models;
using CLWebStore.Catalog.OutboxProcessor.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CLWebStore.Catalog.OutboxProcessor.Functions;

public class OutboxProcessorFunction
{
    private readonly IEventPublisher _publisher;
    private readonly IDeadLetterStore _deadLetterStore;
    private readonly ILogger<OutboxProcessorFunction> _logger;

    public OutboxProcessorFunction(
        IEventPublisher publisher,
        IDeadLetterStore deadLetterStore,
        ILogger<OutboxProcessorFunction> logger)
    {
        _publisher = publisher;
        _deadLetterStore = deadLetterStore;
        _logger = logger;
    }

    [Function(nameof(OutboxProcessorFunction))]
    public async Task Run(
        [CosmosDBTrigger(
            // Use % % to pull from environment variables / app settings
            databaseName: "%CosmosDbDatabaseName%",
            containerName: "%CosmosDbContainerName%",
            Connection = "CosmosDBConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true,
            StartFromBeginning = true)] IReadOnlyList<JsonDocument> input)
    {
        if (input == null || input.Count == 0)
            return;

        foreach (var doc in input)
        {
            try
            {
                // 1. Inspect the JSON to see what kind of document this is
                if (doc.RootElement.TryGetProperty("type", out var typeElement))
                {
                    var documentType = typeElement.GetString();

                    // 2. ONLY process Outbox events. Ignore Product documents entirely.
                    // Make sure "OutboxEvent" matches whatever you set in ProductMapper.ToOutboxDocument()
                    if (documentType != "OutboxEvent")
                    {
                        continue;
                    }
                }
                else
                {
                    // If there is no 'type' property, skip it
                    continue;
                }

                // 3. Deserialize safely now that we know it's an Outbox event
                var outboxMessage = doc.Deserialize<OutboxMessage>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (outboxMessage == null)
                    continue;

                // 4. Attempt to publish
                await _publisher.PublishAsync(outboxMessage);
                _logger.LogInformation($"Successfully published message {outboxMessage.Id}.");
            }
            catch (Exception ex)
            {
                // Extract ID and Type for the DLQ if possible, otherwise use fallbacks
                var docId = doc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : Guid.NewGuid().ToString();
                var docType = doc.RootElement.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : "Unknown";

                _logger.LogError(ex, $"Failed to process message {docId}. Archiving to DLQ.");

                // Capture the failure to Table Storage
                await _deadLetterStore.CaptureAsync(doc.RootElement.GetRawText(), ex, docId, docType);
            }
        }
    }
}
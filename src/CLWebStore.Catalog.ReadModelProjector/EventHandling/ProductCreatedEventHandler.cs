using CLWebStore.Catalog.ReadModelProjector.Models;
using CLWebStore.Catalog.ReadModelProjector.Projection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CLWebStore.Catalog.ReadModelProjector.EventHandling;

public sealed class ProductCreatedEventHandler : IProductEventHandler
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<ProductCreatedEventHandler> _logger;
    private readonly IProductProjectionService _projectionService;

    public ProductCreatedEventHandler(
        JsonSerializerOptions jsonSerializerOptions,
        IProductProjectionService projectionService,
        ILogger<ProductCreatedEventHandler> logger)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _projectionService = projectionService;
        _logger = logger;
    }

    public string EventType => "ProductCreatedEvent";

    public async Task HandleAsync(OutboxMessageWrapper wrapper, CancellationToken cancellationToken)
    {
        ProductDomainEvent productEvent;

        try
        {
            productEvent = JsonSerializer.Deserialize<ProductDomainEvent>(wrapper.Payload, _jsonSerializerOptions)
                ?? throw new JsonException("ProductCreatedEvent payload deserialized to null.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize {EventType} payload for outbox message {OutboxMessageId}.",
                wrapper.EventType,
                wrapper.Id);

            throw;
        }

        Activity.Current?.SetTag("catalog.product_id", productEvent.ProductId);

        _logger.LogInformation(
            "Projecting {EventType} for product {ProductId}.",
            wrapper.EventType,
            productEvent.ProductId);

        await _projectionService.UpsertAsync(productEvent, cancellationToken);
    }
}

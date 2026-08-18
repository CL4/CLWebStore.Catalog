using CLWebStore.Catalog.ReadModelProjector.EventHandling;
using CLWebStore.Catalog.ReadModelProjector.Models;
using Microsoft.Extensions.Logging;

namespace CLWebStore.Catalog.ReadModelProjector.Dispatching;

public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IReadOnlyDictionary<string, IProductEventHandler> _handlers;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(
        IEnumerable<IProductEventHandler> handlers,
        ILogger<EventDispatcher> logger)
    {
        _handlers = handlers.ToDictionary(handler => handler.EventType, StringComparer.Ordinal);
        _logger = logger;
    }

    public async Task DispatchAsync(OutboxMessageWrapper wrapper, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(wrapper.EventType, out var handler))
        {
            _logger.LogWarning(
                "Unknown product event type {EventType} for outbox message {OutboxMessageId}.",
                wrapper.EventType,
                wrapper.Id);

            return;
        }

        await handler.HandleAsync(wrapper, cancellationToken);
    }
}

using CLWebStore.Catalog.ReadModelProjector.Models;

namespace CLWebStore.Catalog.ReadModelProjector.Dispatching;

/// <summary>
/// Dispatches outbox messages to event-specific projection handlers.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches an outbox wrapper to the matching event handler.
    /// </summary>
    /// <param name="wrapper">The outbox message wrapper.</param>
    /// <param name="cancellationToken">A token that cancels the dispatch operation.</param>
    Task DispatchAsync(OutboxMessageWrapper wrapper, CancellationToken cancellationToken);
}

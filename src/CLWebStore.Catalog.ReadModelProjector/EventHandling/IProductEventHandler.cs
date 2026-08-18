using CLWebStore.Catalog.ReadModelProjector.Models;

namespace CLWebStore.Catalog.ReadModelProjector.EventHandling;

public interface IProductEventHandler
{
    string EventType { get; }

    Task HandleAsync(OutboxMessageWrapper wrapper, CancellationToken cancellationToken);
}

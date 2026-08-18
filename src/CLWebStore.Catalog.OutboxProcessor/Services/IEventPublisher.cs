using CLWebStore.Catalog.OutboxProcessor.Models;

namespace CLWebStore.Catalog.OutboxProcessor.Services;

public interface IEventPublisher
{
    Task PublishAsync(OutboxMessage message);
}

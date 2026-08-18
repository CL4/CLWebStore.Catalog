namespace CLWebStore.Catalog.OutboxProcessor.Services;

public interface IDeadLetterStore
{
    Task CaptureAsync(object outboxItem, Exception ex, string messageId, string messageType);
}

using Azure.Messaging.ServiceBus;

using CLWebStore.Catalog.OutboxProcessor.Models;

namespace CLWebStore.Catalog.OutboxProcessor.Services;

public class ServiceBusPublisher : IEventPublisher
{
    private readonly ServiceBusSender _sender;

    public ServiceBusPublisher(ServiceBusClient serviceBusClient)
    {
        // ServiceBusClient handles connection pooling. The sender is created for a specific topic/queue.
        _sender = serviceBusClient.CreateSender("catalog-events-topic");
    }

    public async Task PublishAsync(OutboxMessage message)
    {
        var serviceBusMessage = new ServiceBusMessage(message.Payload)
        {
            MessageId = message.Id,
            Subject = message.Type
        };

        // Transient retries are handled natively by ServiceBusClientOptions configured in Program.cs
        await _sender.SendMessageAsync(serviceBusMessage);
    }
}
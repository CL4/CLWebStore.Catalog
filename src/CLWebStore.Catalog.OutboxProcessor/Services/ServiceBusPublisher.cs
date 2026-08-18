using Azure.Messaging.ServiceBus;
using CLWebStore.Catalog.OutboxProcessor.Models;
using Microsoft.Extensions.Configuration; // Ensure this is imported

namespace CLWebStore.Catalog.OutboxProcessor.Services;

public class ServiceBusPublisher : IEventPublisher
{
    private readonly ServiceBusSender _sender;

    public ServiceBusPublisher(ServiceBusClient serviceBusClient, IConfiguration configuration)
    {
        // 1. Read the topic name from configuration
        var topicName = configuration["ServiceBusTopicName"];

        // 2. Fail fast if the configuration is missing
        if (string.IsNullOrWhiteSpace(topicName))
        {
            throw new InvalidOperationException("Configuration missing: 'ServiceBusTopicName' is not set.");
        }

        // ServiceBusClient handles connection pooling. The sender is created for a specific topic/queue.
        _sender = serviceBusClient.CreateSender(topicName);
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

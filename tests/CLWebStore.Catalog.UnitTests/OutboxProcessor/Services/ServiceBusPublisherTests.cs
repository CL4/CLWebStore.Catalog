using Azure.Messaging.ServiceBus;
using CLWebStore.Catalog.OutboxProcessor.Models;
using CLWebStore.Catalog.OutboxProcessor.Services;
using Moq;

namespace CLWebStore.Catalog.UnitTests.OutboxProcessor.Services;

public class ServiceBusPublisherTests
{
    [Fact]
    public async Task PublishAsync_SendsServiceBusMessage_WithCorrectProperties()
    {
        // Arrange
        var mockSender = new Mock<ServiceBusSender>();
        mockSender
            .Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockClient = new Mock<ServiceBusClient>();
        mockClient.Setup(c => c.CreateSender(It.Is<string>(t => t == "catalog-events-topic"))).Returns(mockSender.Object);

        var publisher = new ServiceBusPublisher(mockClient.Object);

        var outbox = new OutboxMessage { Id = "msg-1", Type = "ProductCreated", Payload = "{}" };

        // Act
        await publisher.PublishAsync(outbox);

        // Assert
        mockSender.Verify(s => s.SendMessageAsync(
            It.Is<ServiceBusMessage>(m => m.MessageId == outbox.Id && m.Subject == outbox.Type),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

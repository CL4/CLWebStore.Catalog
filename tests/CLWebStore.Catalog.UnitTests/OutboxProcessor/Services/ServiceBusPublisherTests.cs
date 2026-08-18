using Azure.Messaging.ServiceBus;
using CLWebStore.Catalog.OutboxProcessor.Models;
using CLWebStore.Catalog.OutboxProcessor.Services;
using Microsoft.Extensions.Configuration;
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
        mockClient
            .Setup(c => c.CreateSender(It.Is<string>(t => t == "catalog-events-topic")))
            .Returns(mockSender.Object);

        // 1. Mock the IConfiguration
        var mockConfig = new Mock<IConfiguration>();
        mockConfig
            .Setup(c => c["ServiceBusTopicName"])
            .Returns("catalog-events-topic");

        // 2. Pass the mocked configuration into the constructor
        var publisher = new ServiceBusPublisher(mockClient.Object, mockConfig.Object);

        var outbox = new OutboxMessage { Id = "msg-1", Type = "ProductCreated", Payload = "{}" };

        // Act
        await publisher.PublishAsync(outbox);

        // Assert
        mockSender.Verify(s => s.SendMessageAsync(
            It.Is<ServiceBusMessage>(m => m.MessageId == outbox.Id && m.Subject == outbox.Type),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenTopicNameIsMissing()
    {
        // Arrange
        var mockClient = new Mock<ServiceBusClient>();
        var mockConfig = new Mock<IConfiguration>();

        // Simulate missing configuration by returning null
        mockConfig.Setup(c => c["ServiceBusTopicName"]).Returns((string)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceBusPublisher(mockClient.Object, mockConfig.Object));
    }
}

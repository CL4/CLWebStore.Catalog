using CLWebStore.Catalog.ReadModelProjector.Dispatching;
using CLWebStore.Catalog.ReadModelProjector.EventHandling;
using CLWebStore.Catalog.ReadModelProjector.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace CLWebStore.Catalog.UnitTests.ReadModelProjector.Dispatching;

public class EventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_KnownEventType_CallsHandlerHandleAsync()
    {
        // Arrange
        var wrapper = new OutboxMessageWrapper { Id = "1", EventType = "ProductCreatedEvent", Payload = "{}" };
        var token = CancellationToken.None;

        var mockHandler = new Mock<IProductEventHandler>();
        mockHandler.SetupGet(h => h.EventType).Returns("ProductCreatedEvent");
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<OutboxMessageWrapper>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var mockLogger = new Mock<ILogger<EventDispatcher>>();

        var dispatcher = new EventDispatcher(new[] { mockHandler.Object }, mockLogger.Object);

        // Act
        await dispatcher.DispatchAsync(wrapper, token);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(It.Is<OutboxMessageWrapper>(w => w == wrapper), It.Is<CancellationToken>(ct => ct == token)), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_UnknownEventType_SkipsAndDoesNotCallHandler()
    {
        // Arrange
        var wrapper = new OutboxMessageWrapper { Id = "1", EventType = "SomeOtherEvent", Payload = "{}" };
        var token = CancellationToken.None;

        var mockHandler = new Mock<IProductEventHandler>();
        mockHandler.SetupGet(h => h.EventType).Returns("ProductCreatedEvent");
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<OutboxMessageWrapper>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<EventDispatcher>>();

        var dispatcher = new EventDispatcher(new[] { mockHandler.Object }, mockLogger.Object);

        // Act
        await dispatcher.DispatchAsync(wrapper, token);

        // Assert: handler should not be invoked
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<OutboxMessageWrapper>(), It.IsAny<CancellationToken>()), Times.Never);
        // No exception thrown
    }
}

using CLWebStore.Catalog.ReadModelProjector.Dispatching;
using CLWebStore.Catalog.ReadModelProjector.Functions;
using CLWebStore.Catalog.ReadModelProjector.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CLWebStore.Catalog.UnitTests.ReadModelProjector.Functions;

public class ProjectionFunctionTests
{
    private static Mock<FunctionContext> CreateFunctionContext(string messageId, string correlationId, int deliveryCount, DateTimeOffset enqueuedTime)
    {
        var bindingData = new Dictionary<string, object?>
        {
            ["MessageId"] = messageId,
            ["CorrelationId"] = correlationId,
            ["DeliveryCount"] = deliveryCount,
            ["EnqueuedTimeUtc"] = enqueuedTime
        } as IReadOnlyDictionary<string, object?>;

        var mockBindingContext = new Mock<BindingContext>();
        mockBindingContext.SetupGet(b => b.BindingData).Returns(bindingData);

        var mockContext = new Mock<FunctionContext>();
        mockContext.SetupGet(c => c.BindingContext).Returns(mockBindingContext.Object);

        return mockContext;
    }

    [Fact]
    public async Task RunAsync_ValidOutboxWrapper_DispatchesSuccessfully()
    {
        // Arrange
        var wrapper = new OutboxMessageWrapper
        {
            Id = "msg-1",
            EventType = "ProductCreatedEvent",
            Payload = "{}",
            OccurredOn = DateTimeOffset.UtcNow
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(wrapper);
        var message = new BinaryData(bytes);

        var mockDispatcher = new Mock<IEventDispatcher>();
        mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<OutboxMessageWrapper>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var mockLogger = new Mock<ILogger<ProjectionFunction>>();

        var fn = new ProjectionFunction(mockDispatcher.Object, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, mockLogger.Object);

        var ctx = CreateFunctionContext("mid-1", "corr-1", 1, DateTimeOffset.UtcNow);

        // Act
        await fn.RunAsync(message, ctx.Object, CancellationToken.None);

        // Assert
        mockDispatcher.Verify(d => d.DispatchAsync(It.Is<OutboxMessageWrapper>(w => w.Id == wrapper.Id && w.EventType == wrapper.EventType), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_InvalidJsonMessage_ThrowsJsonExceptionAndLogsError()
    {
        // Arrange
        var invalidBytes = System.Text.Encoding.UTF8.GetBytes("not-a-json");
        var message = new BinaryData(invalidBytes);

        var mockDispatcher = new Mock<IEventDispatcher>();
        var mockLogger = new Mock<ILogger<ProjectionFunction>>();

        var fn = new ProjectionFunction(mockDispatcher.Object, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, mockLogger.Object);

        var ctx = CreateFunctionContext("mid-2", "corr-2", 1, DateTimeOffset.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => fn.RunAsync(message, ctx.Object, CancellationToken.None));

        // Dispatcher should not be called
        mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<OutboxMessageWrapper>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

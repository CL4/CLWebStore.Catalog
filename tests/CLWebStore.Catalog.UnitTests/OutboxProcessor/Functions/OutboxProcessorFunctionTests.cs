using CLWebStore.Catalog.OutboxProcessor.Functions;
using CLWebStore.Catalog.OutboxProcessor.Models;
using CLWebStore.Catalog.OutboxProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CLWebStore.Catalog.UnitTests.OutboxProcessor.Functions;

public class OutboxProcessorFunctionTests
{
    private static JsonDocument CreateDoc(string json) => JsonDocument.Parse(json);

    [Fact]
    public async Task Run_NullOrEmptyInput_ReturnsImmediatelyWithoutProcessing()
    {
        var mockPublisher = new Mock<IEventPublisher>();
        var mockDlq = new Mock<IDeadLetterStore>();
        var mockLogger = new Mock<ILogger<OutboxProcessorFunction>>();

        var fn = new OutboxProcessorFunction(mockPublisher.Object, mockDlq.Object, mockLogger.Object);

        // Empty list
        await fn.Run(new List<JsonDocument>());

        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<OutboxMessage>()), Times.Never);
        mockDlq.Verify(d => d.CaptureAsync(It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_NonOutboxDocumentType_SkipsPublishing()
    {
        var mockPublisher = new Mock<IEventPublisher>();
        var mockDlq = new Mock<IDeadLetterStore>();
        var mockLogger = new Mock<ILogger<OutboxProcessorFunction>>();

        var fn = new OutboxProcessorFunction(mockPublisher.Object, mockDlq.Object, mockLogger.Object);

        var json = "{ \"id\": \"1\", \"type\": \"Product\", \"payload\": \"{}\" }";
        var doc = CreateDoc(json);

        await fn.Run(new List<JsonDocument> { doc });

        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<OutboxMessage>()), Times.Never);
        mockDlq.Verify(d => d.CaptureAsync(It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_MissingTypeProperty_SkipsPublishing()
    {
        var mockPublisher = new Mock<IEventPublisher>();
        var mockDlq = new Mock<IDeadLetterStore>();
        var mockLogger = new Mock<ILogger<OutboxProcessorFunction>>();

        var fn = new OutboxProcessorFunction(mockPublisher.Object, mockDlq.Object, mockLogger.Object);

        var json = "{ \"id\": \"1\", \"payload\": \"{}\" }";
        var doc = CreateDoc(json);

        await fn.Run(new List<JsonDocument> { doc });

        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<OutboxMessage>()), Times.Never);
        mockDlq.Verify(d => d.CaptureAsync(It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_ValidOutboxEvent_PublishesSuccessfully()
    {
        var mockPublisher = new Mock<IEventPublisher>();
        mockPublisher.Setup(p => p.PublishAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);

        var mockDlq = new Mock<IDeadLetterStore>();
        var mockLogger = new Mock<ILogger<OutboxProcessorFunction>>();

        var fn = new OutboxProcessorFunction(mockPublisher.Object, mockDlq.Object, mockLogger.Object);

        var id = "msg-1";
        var json = $"{{ \"id\": \"{id}\", \"type\": \"OutboxEvent\", \"payload\": \"{{}}\" }}";
        var doc = CreateDoc(json);

        await fn.Run(new List<JsonDocument> { doc });

        mockPublisher.Verify(p => p.PublishAsync(It.Is<OutboxMessage>(m => m.Id == id && m.Type == "OutboxEvent")), Times.Once);
        mockDlq.Verify(d => d.CaptureAsync(It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_PublisherThrowsException_CapturesToDeadLetterStore()
    {
        var mockPublisher = new Mock<IEventPublisher>();
        mockPublisher.Setup(p => p.PublishAsync(It.IsAny<OutboxMessage>())).ThrowsAsync(new InvalidOperationException("boom"));

        var mockDlq = new Mock<IDeadLetterStore>();
        mockDlq.Setup(d => d.CaptureAsync(It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<OutboxProcessorFunction>>();

        var fn = new OutboxProcessorFunction(mockPublisher.Object, mockDlq.Object, mockLogger.Object);

        var id = "dlq-1";
        var type = "OutboxEvent";
        var json = $"{{ \"id\": \"{id}\", \"type\": \"{type}\", \"payload\": \"{{}}\" }}";
        var doc = CreateDoc(json);

        await fn.Run(new List<JsonDocument> { doc });

        mockDlq.Verify(d => d.CaptureAsync(
            It.Is<string>(s => s.Contains(id) && s.Contains(type)),
            It.Is<Exception>(e => e.Message == "boom"),
            It.Is<string>(mid => mid == id),
            It.Is<string>(mt => mt == type)
        ), Times.Once);
    }
}

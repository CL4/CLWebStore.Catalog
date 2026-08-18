using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using CLWebStore.Catalog.OutboxProcessor.Models;
using CLWebStore.Catalog.OutboxProcessor.Services;
using Moq;

namespace CLWebStore.Catalog.UnitTests.OutboxProcessor.Services;

public class DeadLetterStoreTests
{
    [Fact]
    public async Task CaptureAsync_AddsDeadLetterEntity_WithCorrectPartitionAndRowKey()
    {
        // Arrange
        var mockTableClient = new Mock<TableClient>();

        // Mock non-null Azure Response<TableItem> to satisfy nullable reference types
        mockTableClient
            .Setup(t => t.CreateIfNotExists(It.IsAny<CancellationToken>()))
            .Returns(Mock.Of<Response<TableItem>>());

        mockTableClient
            .Setup(t => t.AddEntityAsync(It.IsAny<DeadLetterEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        var mockService = new Mock<TableServiceClient>();
        mockService
            .Setup(s => s.GetTableClient(It.Is<string>(n => n == "OutboxDeadLetter")))
            .Returns(mockTableClient.Object);

        var store = new DeadLetterStore(mockService.Object);

        var ex = new Exception("boom");
        var id = "id-1";
        var type = "TypeX";
        var payload = new { foo = "bar" };

        // Act
        await store.CaptureAsync(payload, ex, id, type);

        // Assert
        mockTableClient.Verify(t => t.AddEntityAsync(
            It.Is<DeadLetterEntity>(e => e.PartitionKey == type && e.RowKey == id && e.ErrorMessage == ex.Message),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
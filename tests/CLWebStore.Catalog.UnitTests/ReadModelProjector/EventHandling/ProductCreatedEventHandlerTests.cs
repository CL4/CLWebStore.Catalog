using CLWebStore.Catalog.ReadModelProjector.EventHandling;
using CLWebStore.Catalog.ReadModelProjector.Models;
using CLWebStore.Catalog.ReadModelProjector.Projection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CLWebStore.Catalog.UnitTests.ReadModelProjector.EventHandling;

public class ProductCreatedEventHandlerTests
{
    private readonly JsonSerializerOptions _opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task HandleAsync_ValidPayload_CallsProjectionService()
    {
        var productEvent = new ProductDomainEvent { ProductId = Guid.NewGuid(), Sku = "SKU", Name = "N", PriceAmount = 1m, PriceCurrency = "USD", OccurredOn = DateTimeOffset.UtcNow };
        var payload = JsonSerializer.Serialize(productEvent, _opts);

        var wrapper = new OutboxMessageWrapper { Id = "1", EventType = "ProductCreatedEvent", Payload = payload };
        var token = CancellationToken.None;

        var mockProjection = new Mock<IProductProjectionService>();
        mockProjection.Setup(p => p.UpsertAsync(It.IsAny<ProductDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var mockLogger = new Mock<ILogger<ProductCreatedEventHandler>>();

        var handler = new ProductCreatedEventHandler(_opts, mockProjection.Object, mockLogger.Object);

        await handler.HandleAsync(wrapper, token);

        mockProjection.Verify(p => p.UpsertAsync(It.Is<ProductDomainEvent>(e => e.ProductId == productEvent.ProductId), It.Is<CancellationToken>(ct => ct == token)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_InvalidJson_ThrowsJsonException_And_Logs()
    {
        var wrapper = new OutboxMessageWrapper { Id = "1", EventType = "ProductCreatedEvent", Payload = "{ not-json }" };
        var token = CancellationToken.None;

        var mockProjection = new Mock<IProductProjectionService>();
        var mockLogger = new Mock<ILogger<ProductCreatedEventHandler>>();

        var handler = new ProductCreatedEventHandler(_opts, mockProjection.Object, mockLogger.Object);

        await Assert.ThrowsAsync<JsonException>(() => handler.HandleAsync(wrapper, token));

        // Ensure projection service not called
        mockProjection.Verify(p => p.UpsertAsync(It.IsAny<ProductDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

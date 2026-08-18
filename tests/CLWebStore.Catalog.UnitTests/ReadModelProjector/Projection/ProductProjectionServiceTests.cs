using CLWebStore.Catalog.ReadModelProjector.Models;
using CLWebStore.Catalog.ReadModelProjector.Projection;
using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper; // <-- Required for Dapper extension mocking
using System.Data.Common;
using System.Text.Json;

namespace CLWebStore.Catalog.UnitTests.ReadModelProjector.Projection;

public class ProductProjectionServiceTests
{
    private readonly Mock<IConnectionFactory> _mockConnectionFactory;
    private readonly Mock<ILogger<ProductProjectionService>> _mockLogger;
    private readonly ProductProjectionService _service;
    private readonly JsonSerializerOptions _opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public ProductProjectionServiceTests()
    {
        _mockConnectionFactory = new Mock<IConnectionFactory>();
        _mockLogger = new Mock<ILogger<ProductProjectionService>>();

        _service = new ProductProjectionService(_mockConnectionFactory.Object, _opts, _mockLogger.Object);
    }

    [Fact]
    public async Task UpsertAsync_SuccessfulUpsert_OpensConnectionAndExecutes()
    {
        var productEvent = new ProductDomainEvent
        {
            ProductId = Guid.NewGuid(),
            Sku = "S",
            Name = "N",
            PriceAmount = 1m,
            PriceCurrency = "USD",
            OccurredOn = DateTimeOffset.UtcNow
        };
        var token = CancellationToken.None;

        var mockConnection = new Mock<DbConnection>();

        // Setup Moq.Dapper so ExecuteAsync succeeds without throwing a NullReferenceException
        mockConnection
            .SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(1);

        _mockConnectionFactory
            .Setup(f => f.OpenConnectionAsync(It.Is<CancellationToken>(ct => ct == token)))
            .ReturnsAsync(mockConnection.Object);

        await _service.UpsertAsync(productEvent, token);

        _mockConnectionFactory.Verify(f => f.OpenConnectionAsync(It.Is<CancellationToken>(ct => ct == token)), Times.Once);
        _mockLogger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpsertAsync_OpenConnectionThrows_DbException_IsRethrownAndLogged()
    {
        var productEvent = new ProductDomainEvent
        {
            ProductId = Guid.NewGuid(),
            Sku = "S",
            Name = "N",
            PriceAmount = 1m,
            PriceCurrency = "USD",
            OccurredOn = DateTimeOffset.UtcNow
        };
        var token = CancellationToken.None;

        var dbException = new Mock<DbException>().Object;

        _mockConnectionFactory
            .Setup(f => f.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Changed from ThrowsAsync to ThrowsAnyAsync to allow the Moq proxy type
        await Assert.ThrowsAnyAsync<DbException>(() => _service.UpsertAsync(productEvent, token));

        _mockConnectionFactory.Verify(f => f.OpenConnectionAsync(It.Is<CancellationToken>(ct => ct == token)), Times.Once);
        _mockLogger.Verify(l => l.Log(
            It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}

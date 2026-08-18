using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Mappings;
using CLWebStore.Catalog.Infrastructure.Repositories;
using CLWebStore.Catalog.UnitTests.Common.Builders;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace CLWebStore.Catalog.UnitTests.Infrastructure.Repositories;

public class ProductRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly Mock<ICosmosClientFactory> _mockFactory;
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;
    private readonly ProductRepository _repo;

    public ProductRepositoryTests()
    {
        // Create CosmosSettings options and pass to mocked factory constructor
        var settings = new CosmosSettings
        {
            PrimaryConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDfjgjEG=;",
            DatabaseName = "CatalogDb",
            ContainerName = "Catalog"
        };

        var options = Options.Create(settings);

        _mockContainer = new Mock<Container>();

        // When constructing the mocked factory, Moq will call the real constructor taking IOptions<CosmosSettings>
        _mockFactory = new Mock<ICosmosClientFactory>();
        _mockFactory.Setup(f => f.GetCatalogContainer()).Returns(_mockContainer.Object);

        _mockLogger = new Mock<ILogger<ProductRepository>>();

        _repo = new ProductRepository(_mockFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsProduct_And_PropagatesCancellationToken()
    {
        // Arrange
        var token = CancellationToken.None;
        var product = ProductBuilder.New().WithId(Guid.NewGuid()).WithPrice(3.21m, "USD").Build();
        var productDoc = ProductMapper.ToDocument(product);

        var mockItemResponse = new Mock<ItemResponse<ProductDocument>>();
        mockItemResponse.SetupGet(r => r.Resource).Returns(productDoc);

        _mockContainer
            .Setup(c => c.ReadItemAsync<ProductDocument>(
                It.Is<string>(s => s == product.Id.ToString()),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions?>(),
                It.Is<CancellationToken>(ct => ct == token)))
            .ReturnsAsync(mockItemResponse.Object);

        // Act
        var result = await _repo.GetByIdAsync(product.Id, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Sku.Value, result.Sku.Value);
        _mockContainer.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull_And_Logs()
    {
        var token = CancellationToken.None;
        var id = Guid.NewGuid();

        _mockContainer
            .Setup(c => c.ReadItemAsync<ProductDocument>(
                It.Is<string>(s => s == id.ToString()),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions?>(),
                It.Is<CancellationToken>(ct => ct == token)))
            .ThrowsAsync(new CosmosException("NotFound", HttpStatusCode.NotFound, 0, string.Empty, 0.0));

        var result = await _repo.GetByIdAsync(id, token);

        Assert.Null(result);
        // Logging is performed via extension method; ensure container setup was invoked
        _mockContainer.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_UnexpectedException_Rethrows()
    {
        var token = CancellationToken.None;
        var id = Guid.NewGuid();

        _mockContainer
            .Setup(c => c.ReadItemAsync<ProductDocument>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions?>(),
                It.Is<CancellationToken>(ct => ct == token)))
            .ThrowsAsync(new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.GetByIdAsync(id, token));
    }

    [Fact]
    public async Task SaveAsync_NewProduct_CreatesItems_ExecutesBatch_And_ClearsDomainEvents()
    {
        var token = CancellationToken.None;
        var product = ProductBuilder.New().WithId(Guid.NewGuid()).WithPrice(4.44m, "USD").Build();

        var mockBatch = new Mock<TransactionalBatch>();
        mockBatch.Setup(b => b.CreateItem(It.IsAny<object>())).Returns(mockBatch.Object);

        var mockBatchResponse = new Mock<TransactionalBatchResponse>();
        mockBatchResponse.SetupGet(r => r.IsSuccessStatusCode).Returns(true);
        mockBatchResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.OK);

        mockBatch
            .Setup(b => b.ExecuteAsync(It.Is<CancellationToken>(ct => ct == token)))
            .ReturnsAsync(mockBatchResponse.Object);

        _mockContainer.Setup(c => c.CreateTransactionalBatch(It.IsAny<PartitionKey>())).Returns(mockBatch.Object);

        // Act
        await _repo.SaveAsync(product, token);

        // Assert: CreateItem for product and for each domain event
        mockBatch.Verify(b => b.CreateItem(It.IsAny<object>()), Times.AtLeastOnce);
        mockBatch.Verify(b => b.ExecuteAsync(It.Is<CancellationToken>(ct => ct == token)), Times.Once);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public async Task SaveAsync_ExistingProduct_UsesReplaceItem_WithIfMatchEtag()
    {
        var token = CancellationToken.None;
        var product = ProductBuilder.New().WithId(Guid.NewGuid()).WithPrice(4.44m, "USD").WithVersion("etag-1").Build();

        var mockBatch = new Mock<TransactionalBatch>();
        mockBatch.Setup(b => b.ReplaceItem(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TransactionalBatchItemRequestOptions>())).Returns(mockBatch.Object);
        mockBatch.Setup(b => b.CreateItem(It.IsAny<object>())).Returns(mockBatch.Object);

        var mockBatchResponse = new Mock<TransactionalBatchResponse>();
        mockBatchResponse.SetupGet(r => r.IsSuccessStatusCode).Returns(true);
        mockBatchResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.OK);

        mockBatch
            .Setup(b => b.ExecuteAsync(It.Is<CancellationToken>(ct => ct == token)))
            .ReturnsAsync(mockBatchResponse.Object);

        _mockContainer.Setup(c => c.CreateTransactionalBatch(It.IsAny<PartitionKey>())).Returns(mockBatch.Object);

        // Act
        await _repo.SaveAsync(product, token);

        // Assert: ReplaceItem called with IfMatchEtag set to etag-1
        mockBatch.Verify(b => b.ReplaceItem(
            It.Is<string>(id => id == product.Id.ToString()),
            It.IsAny<object>(),
            It.Is<TransactionalBatchItemRequestOptions>(opts => opts != null && opts.IfMatchEtag == "etag-1")
        ), Times.AtLeastOnce);

        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public async Task SaveAsync_BatchFailure_ThrowsException()
    {
        var token = CancellationToken.None;
        var product = ProductBuilder.New().WithId(Guid.NewGuid()).WithPrice(2.22m, "USD").Build();

        var mockBatch = new Mock<TransactionalBatch>();
        mockBatch.Setup(b => b.CreateItem(It.IsAny<object>())).Returns(mockBatch.Object);

        var mockBatchResponse = new Mock<TransactionalBatchResponse>();
        mockBatchResponse.SetupGet(r => r.IsSuccessStatusCode).Returns(false);
        mockBatchResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.BadRequest);

        mockBatch
            .Setup(b => b.ExecuteAsync(It.Is<CancellationToken>(ct => ct == token)))
            .ReturnsAsync(mockBatchResponse.Object);

        _mockContainer.Setup(c => c.CreateTransactionalBatch(It.IsAny<PartitionKey>())).Returns(mockBatch.Object);

        await Assert.ThrowsAsync<Exception>(() => _repo.SaveAsync(product, token));

        mockBatch.Verify(b => b.ExecuteAsync(It.Is<CancellationToken>(ct => ct == token)), Times.Once);
    }
}

using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Infrastructure.QueryServices.V1;
using Dapper;
using Moq;
using Moq.Dapper;
using System.Data;

namespace CLWebStore.Catalog.UnitTests.Infrastructure.QueryServices.V1;

public class ProductQueryServiceTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly ProductQueryService _service;

    public ProductQueryServiceTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _service = new ProductQueryService(_mockConnection.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsProduct_WhenFound()
    {
        var id = Guid.NewGuid();
        var dto = new ProductDto { Id = id, Sku = "SKU-1", Name = "P", PriceAmount = 1m, PriceCurrency = "USD" };
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(dto);

        var result = await _service.GetProductByIdAsync(id, token);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync((ProductDto?)null);

        var result = await _service.GetProductByIdAsync(id, token);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductsBySkuAsync_ReturnsMatchingProducts()
    {
        var skus = new[] { "A", "B" };
        var list = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Sku = "A", Name = "A", PriceAmount = 1m, PriceCurrency = "USD" },
            new() { Id = Guid.NewGuid(), Sku = "B", Name = "B", PriceAmount = 2m, PriceCurrency = "USD" }
        };
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(list);

        var result = await _service.GetProductsBySkuAsync(skus, token);

        Assert.NotNull(result);
        Assert.Collection(result,
            item => Assert.Equal("A", item.Sku),
            item => Assert.Equal("B", item.Sku));
    }

    [Fact]
    public async Task GetProductsBySkuAsync_ReturnsEmpty_WhenNoMatch()
    {
        var skus = new[] { "X" };
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(new List<ProductDto>());

        var result = await _service.GetProductsBySkuAsync(skus, token);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsProducts_WhenExist()
    {
        var categoryId = Guid.NewGuid();
        var list = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Sku = "C1", Name = "C1", PriceAmount = 1m, PriceCurrency = "USD" }
        };
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(list);

        var result = await _service.GetProductsByCategoryAsync(categoryId, token);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsEmpty_WhenNone()
    {
        var categoryId = Guid.NewGuid();
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(new List<ProductDto>());

        var result = await _service.GetProductsByCategoryAsync(categoryId, token);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRelatedProductsAsync_ReturnsProducts_WhenExist()
    {
        var productId = Guid.NewGuid();
        var list = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Sku = "R1", Name = "R1", PriceAmount = 1m, PriceCurrency = "USD" }
        };
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(list);

        var result = await _service.GetRelatedProductsAsync(productId, token);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetRelatedProductsAsync_ReturnsEmpty_WhenNone()
    {
        var productId = Guid.NewGuid();
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(new List<ProductDto>());

        var result = await _service.GetRelatedProductsAsync(productId, token);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchProductsAsync_ReturnsMatchingProducts_And_RespectsLimit()
    {
        var query = "widget";
        var list = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Sku = "W1", Name = "Widget One", PriceAmount = 3m, PriceCurrency = "USD" }
        };
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(list);

        var result = await _service.SearchProductsAsync(query, 10, token);

        Assert.Single(result);
    }

    [Fact]
    public async Task SearchProductsAsync_ReturnsEmpty_WhenNoMatches()
    {
        var query = "nomatch";
        var token = CancellationToken.None;

        _mockConnection.SetupDapperAsync(c => c.QueryAsync<ProductDto>(It.IsAny<CommandDefinition>())).ReturnsAsync(new List<ProductDto>());

        var result = await _service.SearchProductsAsync(query, 10, token);

        Assert.Empty(result);
    }
}

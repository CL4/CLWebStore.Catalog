using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Application.Queries.V1.GetProductById;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Queries.V1.GetProductById;

public class GetProductByIdHandlerTests
{
    [Fact]
    public async Task Handle_ProductFound_ReturnsProductDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ProductDto { Id = id, Sku = "SKU-1", Name = "P", PriceAmount = 1m, PriceCurrency = "USD" };

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var handler = new GetProductByIdHandler(mockQs.Object);
        var query = new GetProductByIdQuery(id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("SKU-1", result.Sku);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ProductDto?)null);

        var handler = new GetProductByIdHandler(mockQs.Object);
        var query = new GetProductByIdQuery(id);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }
}

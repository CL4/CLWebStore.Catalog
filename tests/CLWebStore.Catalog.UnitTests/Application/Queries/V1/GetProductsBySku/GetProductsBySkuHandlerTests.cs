using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Application.Queries.V1.GetProductsBySku;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Queries.V1.GetProductsBySku;

public class GetProductsBySkuHandlerTests
{
    [Fact]
    public async Task Handle_ProductsFound_ReturnsList()
    {
        var skus = new List<string> { "SKU-A", "SKU-B" };
        var list = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Sku = "SKU-A", Name = "A", PriceAmount = 1m, PriceCurrency = "USD" },
            new() { Id = Guid.NewGuid(), Sku = "SKU-B", Name = "B", PriceAmount = 2m, PriceCurrency = "USD" }
        };

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetProductsBySkuAsync(It.Is<IEnumerable<string>>(s => s != null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var handler = new GetProductsBySkuHandler(mockQs.Object);
        var query = new GetProductsBySkuQuery(skus);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Collection(result,
            item => Assert.Equal("SKU-A", item.Sku),
            item => Assert.Equal("SKU-B", item.Sku));
    }

    [Fact]
    public async Task Handle_NoProducts_ThrowsNotFoundException()
    {
        var skus = new List<string> { "SKU-X" };
        var mockQs = new Mock<IProductQueryService>();
        // Return null to trigger the handler's null coalescing
        mockQs.Setup(x => x.GetProductsBySkuAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<ProductDto>?)null);

        var handler = new GetProductsBySkuHandler(mockQs.Object);
        var query = new GetProductsBySkuQuery(skus);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }
}

using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Queries.V1.GetProductsByCategory;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Queries.V1.GetProductsByCategory;

public class GetProductsByCategoryHandlerTests
{
    [Fact]
    public async Task Handle_ProductsExist_ReturnsCollection()
    {
        var categoryId = Guid.NewGuid();
        var list = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Sku = "SKU-1", Name = "One", PriceAmount = 1m, PriceCurrency = "USD" },
            new() { Id = Guid.NewGuid(), Sku = "SKU-2", Name = "Two", PriceAmount = 2m, PriceCurrency = "USD" }
        };

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var handler = new GetProductsByCategoryHandler(mockQs.Object);
        var query = new GetProductsByCategoryQuery(categoryId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Collection(result,
            item => Assert.Equal("SKU-1", item.Sku),
            item => Assert.Equal("SKU-2", item.Sku));
    }

    [Fact]
    public async Task Handle_NoProducts_ReturnsEmptyCollection()
    {
        var categoryId = Guid.NewGuid();

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        var handler = new GetProductsByCategoryHandler(mockQs.Object);
        var query = new GetProductsByCategoryQuery(categoryId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Queries.V1.GetRelatedProducts;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Queries.V1.GetRelatedProducts;

public class GetRelatedProductsHandlerTests
{
    [Fact]
    public async Task Handle_RelatedProductsExist_ReturnsCollection()
    {
        var productId = Guid.NewGuid();
        var list = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Sku = "R-1", Name = "R1", PriceAmount = 1m, PriceCurrency = "USD" }
        };

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetRelatedProductsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var handler = new GetRelatedProductsHandler(mockQs.Object);
        var query = new GetRelatedProductsQuery(productId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("R-1", ((List<ProductDto>)result)[0].Sku);
    }

    [Fact]
    public async Task Handle_NoRelatedProducts_ReturnsEmptyCollection()
    {
        var productId = Guid.NewGuid();

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.GetRelatedProductsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        var handler = new GetRelatedProductsHandler(mockQs.Object);
        var query = new GetRelatedProductsQuery(productId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

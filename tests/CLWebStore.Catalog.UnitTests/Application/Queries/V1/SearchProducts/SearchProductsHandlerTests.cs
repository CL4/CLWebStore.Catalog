using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Queries.V1.SearchProducts;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Queries.V1.SearchProducts;

public class SearchProductsHandlerTests
{
    [Fact]
    public async Task Handle_SearchMatches_ReturnsCollection()
    {
        var queryText = "widget";
        var list = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Sku = "W-1", Name = "Widget One", PriceAmount = 3m, PriceCurrency = "USD" }
        };

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.SearchProductsAsync(queryText, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var handler = new SearchProductsHandler(mockQs.Object);
        var query = new SearchProductsQuery(queryText, 10);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains(result, p => p.Name.Contains("Widget", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_NoMatches_ReturnsEmptyCollection()
    {
        var queryText = "nomatch";

        var mockQs = new Mock<IProductQueryService>();
        mockQs.Setup(x => x.SearchProductsAsync(queryText, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        var handler = new SearchProductsHandler(mockQs.Object);
        var query = new SearchProductsQuery(queryText, 10);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

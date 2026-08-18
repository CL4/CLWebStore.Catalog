using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Application.Commands.V1.CreateProduct;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Domain.Aggregates;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Commands.V1.CreateProduct;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_SavesProduct_And_ReturnsId()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        Product? captured = null;

        mockRepo
            .Setup(r => r.SaveAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, ct) => captured = p)
            .Returns(Task.CompletedTask);

        var handler = new CreateProductHandler(mockRepo.Object, TimeProvider.System);

        var cmd = new CreateProductCommand(
            Sku: "SKU-XYZ",
            Name: "Cmd Product",
            PriceAmount: 12.34m,
            PriceCurrency: "USD",
            CategoryIds: new List<Guid> { Guid.NewGuid() },
            RelatedProductIds: new List<Guid> { Guid.NewGuid() },
            Images: new List<CreateProductImageDto> { new CreateProductImageDto("http://img", "alt", true) }
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        mockRepo.Verify(x => x.SaveAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(captured);
        Assert.Equal(result, captured!.Id);
        Assert.Equal("SKU-XYZ", captured.Sku.Value);
        Assert.Equal("Cmd Product", captured.Name.Value);
        Assert.Equal(12.34m, captured.Price.Amount);
        Assert.Single(captured.Images);
    }
}

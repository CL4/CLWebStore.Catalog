using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Domain.Entities;
using CLWebStore.Catalog.UnitTests.Common.Builders;
using Moq;

namespace CLWebStore.Catalog.UnitTests.Application.Commands.V1.UpdateProduct;

public class UpdateProductHandlerTests
{
    [Fact]
    public async Task Handle_ProductExists_UpdatesAndSaves()
    {
        // Arrange
        var existingImage = new ProductImage("http://old", "old", true);
        var product = ProductBuilder.New()
            .WithSku("SKU-1")
            .WithName("Original")
            .WithPrice(5.0m)
            .WithImages(existingImage)
            .Build();

        var mockRepo = new Mock<IProductRepository>();
        Product? saved = null;

        mockRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        mockRepo
            .Setup(r => r.SaveAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, ct) => saved = p)
            .Returns(Task.CompletedTask);

        var handler = new UpdateProductHandler(mockRepo.Object, TimeProvider.System);

        var cmd = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Updated",
            PriceAmount = 10m,
            PriceCurrency = "USD",
            CategoryIds = new List<Guid>(),
            RelatedProductIds = new List<Guid>(),
            Images =
            [
                // Update existing image
                new(existingImage.Id, "http://old-upd", "old-upd", false),
                // Add new image
                new(null, "http://new", "new", true)
            ]
        };

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        mockRepo.Verify(r => r.SaveAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(saved);
        Assert.Equal("Updated", saved!.Name.Value);
        Assert.Equal(10m, saved.Price.Amount);
        Assert.Equal(2, saved.Images.Count);
        Assert.Contains(saved.Images, i => i.Url == "http://old-upd");
        Assert.Contains(saved.Images, i => i.Url == "http://new");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var mockRepo = new Mock<IProductRepository>();
        mockRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new UpdateProductHandler(mockRepo.Object, TimeProvider.System);

        var cmd = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "X",
            PriceAmount = 1m,
            PriceCurrency = "USD"
        };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SaveThrowsConcurrencyException_BubblesUp()
    {
        var product = ProductBuilder.New().Build();

        var mockRepo = new Mock<IProductRepository>();
        mockRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        mockRepo
            .Setup(r => r.SaveAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("conflict"));

        var handler = new UpdateProductHandler(mockRepo.Object, TimeProvider.System);

        var cmd = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "N",
            PriceAmount = 2m,
            PriceCurrency = "USD"
        };

        await Assert.ThrowsAsync<ConcurrencyException>(() => handler.Handle(cmd, CancellationToken.None));
    }
}

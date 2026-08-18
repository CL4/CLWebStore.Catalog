using CLWebStore.Catalog.Domain.Base;
using CLWebStore.Catalog.Domain.Events;
using CLWebStore.Catalog.Domain.ValueObjects;
using CLWebStore.Catalog.UnitTests.Common.Builders;

namespace CLWebStore.Catalog.UnitTests.Domain.Aggregates;

public class ProductTests
{
    [Fact]
    public void Create_Raises_ProductCreatedEvent()
    {
        var product = ProductBuilder.New()
            .WithSku("SKU-123")
            .WithName("Test Product")
            .WithPrice(5.0m)
            .Build();

        Assert.NotNull(product);
        Assert.NotEmpty(product.DomainEvents);
        Assert.Contains(product.DomainEvents, e => e is ProductCreatedEvent);

        var created = product.DomainEvents.First(e => e is ProductCreatedEvent) as ProductCreatedEvent;
        Assert.Equal(product.Id, created!.ProductId);
        Assert.Equal(product.Sku.ToString(), created.Sku);
    }

    [Fact]
    public void UpdateDetails_MutatesState_And_Appends_ProductUpdatedEvent()
    {
        var product = ProductBuilder.New().Build();

        // clear creation events so we can assert on the update only
        product.ClearDomainEvents();

        var newName = new ProductName("Updated Name");
        var newPrice = new Money(15.50m, "USD");
        var occurredOn = DateTimeOffset.UtcNow;

        product.UpdateDetails(newName, newPrice, occurredOn);

        Assert.Equal("Updated Name", product.Name.Value);
        Assert.Equal(15.50m, product.Price.Amount);
        Assert.Contains(product.DomainEvents, e => e is ProductUpdatedEvent);
    }

    [Fact]
    public void AddAndUpdateImage_ModifiesState_And_AppendsEvents()
    {
        var product = ProductBuilder.New().Build();

        product.ClearDomainEvents();

        var occurredOn = DateTimeOffset.UtcNow;
        product.AddImage("http://example.com/1.jpg", "alt1", true, occurredOn);

        Assert.Single(product.Images);
        Assert.Contains(product.DomainEvents, e => e is ProductUpdatedEvent);

        // update the image
        var imageId = product.Images.First().Id;
        product.ClearDomainEvents();

        product.UpdateImage(imageId, "http://example.com/1-updated.jpg", "alt1-upd", false, DateTimeOffset.UtcNow);

        var img = product.Images.First(i => i.Id == imageId);
        Assert.Equal("http://example.com/1-updated.jpg", img.Url);
        Assert.Equal("alt1-upd", img.AltText);
        Assert.False(img.IsPrimary);
        Assert.Contains(product.DomainEvents, e => e is ProductUpdatedEvent);
    }

    [Fact]
    public void AddRelatedProduct_ToSelf_ThrowsDomainException()
    {
        var product = ProductBuilder.New().Build();

        Assert.Throws<DomainException>(() => product.AddRelatedProduct(product.Id, DateTimeOffset.UtcNow));
    }
}

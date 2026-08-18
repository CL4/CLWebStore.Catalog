using CLWebStore.Catalog.Domain.Base;
using CLWebStore.Catalog.Domain.Entities;

namespace CLWebStore.Catalog.UnitTests.Domain.Entities;

public class ProductImageTests
{
    [Fact]
    public void Create_ValidImage_Succeeds()
    {
        var img = new ProductImage("http://example.com/img.jpg", "An image", true);

        Assert.NotEqual(Guid.Empty, img.Id);
        Assert.Equal("http://example.com/img.jpg", img.Url);
        Assert.Equal("An image", img.AltText);
        Assert.True(img.IsPrimary);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidUrl_ThrowsDomainException(string url)
    {
        Assert.Throws<DomainException>(() => new ProductImage(url, "alt", false));
    }

    [Fact]
    public void Update_InvalidUrl_ThrowsDomainException()
    {
        var img = new ProductImage("http://example.com/img.jpg", "alt", false);

        Assert.Throws<DomainException>(() => img.Update("", "alt2", true));
    }
}

using CLWebStore.Catalog.Domain.Base;
using CLWebStore.Catalog.Domain.ValueObjects;

namespace CLWebStore.Catalog.UnitTests.Domain.ValueObjects;

public class SkuTests
{
    [Fact]
    public void Create_ValidSku_Succeeds_And_IsUppercased()
    {
        var s = new Sku("abc-123");

        Assert.Equal("ABC-123", s.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidSku_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => new Sku(value));
    }

    [Fact]
    public void Equals_SameValue_AreEqual()
    {
        var a = new Sku("sku-1");
        var b = new Sku("SKU-1");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_AreNotEqual()
    {
        var a = new Sku("sku-1");
        var b = new Sku("sku-2");

        Assert.False(a == b);
        Assert.False(a.Equals(b));
    }
}

using CLWebStore.Catalog.Domain.Base;
using CLWebStore.Catalog.Domain.ValueObjects;

namespace CLWebStore.Catalog.UnitTests.Domain.ValueObjects;

public class ProductNameTests
{
    [Fact]
    public void Create_ValidName_Succeeds()
    {
        var n = new ProductName("My Product");

        Assert.Equal("My Product", n.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmpty_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => new ProductName(value));
    }

    [Fact]
    public void Create_TooLong_ThrowsDomainException()
    {
        var longString = new string('a', 201);
        Assert.Throws<DomainException>(() => new ProductName(longString));
    }

    [Fact]
    public void Equals_SameValue_AreEqual()
    {
        var a = new ProductName("Name1");
        var b = new ProductName("Name1");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_AreNotEqual()
    {
        var a = new ProductName("Name1");
        var b = new ProductName("Name2");

        Assert.False(a == b);
        Assert.False(a.Equals(b));
    }
}

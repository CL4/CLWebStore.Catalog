using CLWebStore.Catalog.Domain.Base;
using CLWebStore.Catalog.Domain.ValueObjects;

namespace CLWebStore.Catalog.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ValidMoney_Succeeds()
    {
        var m = new Money(10.50m, "USD");

        Assert.Equal(10.50m, m.Amount);
        Assert.Equal("USD", m.Currency);
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Money(-0.01m, "USD"));
    }

    [Fact]
    public void Equals_SameValues_AreEqual()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(9.99m, "USD");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_AreNotEqual()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(19.99m, "USD");

        Assert.False(a == b);
        Assert.False(a.Equals(b));
    }
}

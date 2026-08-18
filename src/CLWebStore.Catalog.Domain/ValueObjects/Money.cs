using CLWebStore.Catalog.Domain.Base;

namespace CLWebStore.Catalog.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount
    {
        get;
    }
    public string Currency
    {
        get;
    }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Invalid price");
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    // Overriding the base implementation when specific formatting is required.
    public override string ToString() => $"{Amount} {Currency}";
}

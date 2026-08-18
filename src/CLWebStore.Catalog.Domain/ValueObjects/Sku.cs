using CLWebStore.Catalog.Domain.Base;

namespace CLWebStore.Catalog.Domain.ValueObjects;

public class Sku : ValueObject
{
    public string Value
    {
        get;
    }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("SKU cannot be empty");

        Value = value.ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

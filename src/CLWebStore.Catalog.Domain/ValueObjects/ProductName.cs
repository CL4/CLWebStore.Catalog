using CLWebStore.Catalog.Domain.Base;

namespace CLWebStore.Catalog.Domain.ValueObjects;

public class ProductName : ValueObject
{
    public string Value
    {
        get;
    }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Name cannot be empty");

        if (value.Length > 200)
            throw new DomainException("Name too long");

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

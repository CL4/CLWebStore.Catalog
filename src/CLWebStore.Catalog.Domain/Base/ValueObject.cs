namespace CLWebStore.Catalog.Domain.Base;

public abstract class ValueObject
{
    /// <summary>
    /// Gets the components that define the equality of the value object.
    /// Derived classes should override this method to return the properties that should be used for equality comparison.
    /// </summary>
    /// <returns>An enumerable of objects representing the equality components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject? a, ValueObject? b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        var components = GetEqualityComponents()
            .Where(c => c != null)
            .Select(c => c!.ToString());

        // Joins multiple components with a comma, 
        // or just returns the single string for single-value objects.
        return string.Join(", ", components);
    }
}

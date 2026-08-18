using CLWebStore.Catalog.Domain.Base;

namespace CLWebStore.Catalog.Domain.Entities;

public class ProductImage : Entity
{
    public string Url
    {
        get; private set;
    }
    public string AltText
    {
        get; private set;
    }
    public bool IsPrimary
    {
        get; private set;
    }

    public ProductImage(string url, string altText, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL required");

        Id = Guid.NewGuid();
        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
    }

    // Private constructor used for rehydration by infrastructure
    private ProductImage(Guid id, string url, string altText, bool isPrimary)
    {
        Id = id;
        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
    }

    // Rehydration factory method: recreate an instance with an existing Id (for loading from storage)
    public static ProductImage Rehydrate(Guid id, string url, string altText, bool isPrimary)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id must be provided for rehydration", nameof(id));

        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL required");

        return new ProductImage(id, url, altText, isPrimary);
    }

    public void Update(string url, string altText, bool isPrimary)
    {
        // Enforce domain invariants during updates
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL required");

        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
    }
}

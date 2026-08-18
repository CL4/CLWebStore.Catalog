namespace CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;

public class ProductDocument : BaseDocument
{
    public override string Type => "Product";

    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PriceAmount
    {
        get; set;
    }
    public string PriceCurrency { get; set; } = string.Empty;

    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> RelatedProductIds { get; set; } = [];
    public List<ProductImageDocument> Images { get; set; } = [];
}

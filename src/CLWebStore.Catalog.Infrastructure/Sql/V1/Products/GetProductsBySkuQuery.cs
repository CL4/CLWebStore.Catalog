namespace CLWebStore.Catalog.Infrastructure.Sql.V1.Products;

public static class GetProductsBySkuQuery
{
    public const string Sql = """
        SELECT
            id                AS Id,
            sku               AS Sku,
            name              AS Name,
            price_amount      AS PriceAmount,
            price_currency    AS PriceCurrency,
            version           AS Version,
            category_ids      AS CategoryIds,
            related_product_ids AS RelatedProductIds,
            images            AS Images
        FROM read_schema.Products
        WHERE sku = ANY(@Skus)
        """;

    public sealed record Parameters(IEnumerable<string> Skus);
}

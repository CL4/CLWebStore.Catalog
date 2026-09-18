namespace CLWebStore.Catalog.Infrastructure.Sql.V1.Products;

public static class SearchProductsQuery
{
    public const string Sql = """
        SELECT
            id                AS Id,
            sku               AS Sku,
            name              AS Name,
            priceamount       AS PriceAmount,
            pricecurrency     AS PriceCurrency,
            version           AS Version,
            categoryids       AS CategoryIds,
            relatedproductids AS RelatedProductIds,
            images            AS Images
        FROM read_schema.Products
        WHERE name ILIKE '%' || @Query || '%' OR sku ILIKE '%' || @Query || '%'
        ORDER BY name
        LIMIT @Limit
        """;

    public sealed record Parameters(string Query, int Limit);
}

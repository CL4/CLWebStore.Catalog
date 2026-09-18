namespace CLWebStore.Catalog.Infrastructure.Sql.V1.Products;

public static class GetRelatedProductsQuery
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
        WHERE id = ANY(
            SELECT unnest(relatedproductids)
            FROM read_schema.Products
            WHERE id = @ProductId
        )
        """;

    public sealed record Parameters(Guid ProductId);
}

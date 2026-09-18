namespace CLWebStore.Catalog.Infrastructure.Sql.V1.Products;

public static class GetProductByIdQuery
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
        WHERE id = @Id
        """;

    public sealed record Parameters(Guid Id);
}

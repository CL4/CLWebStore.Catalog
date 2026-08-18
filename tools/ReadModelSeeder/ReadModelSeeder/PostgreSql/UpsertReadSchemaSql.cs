namespace ReadModelSeeder.PostgreSql;

public static class UpsertReadSchemaSql
{
    public const string Sql = """
        INSERT INTO read_schema.Products (
            Id,
            Sku,
            Name,
            PriceAmount,
            PriceCurrency,
            Version,
            CategoryIds,
            RelatedProductIds,
            Images
        )
        VALUES (
            @Id,
            @Sku,
            @Name,
            @PriceAmount,
            @PriceCurrency,
            @Version,
            @CategoryIds,
            @RelatedProductIds,
            @Images
        )
        ON CONFLICT (Id)
        DO UPDATE SET
            Sku = EXCLUDED.Sku,
            Name = EXCLUDED.Name,
            PriceAmount = EXCLUDED.PriceAmount,
            PriceCurrency = EXCLUDED.PriceCurrency,
            Version = EXCLUDED.Version,
            CategoryIds = EXCLUDED.CategoryIds,
            RelatedProductIds = EXCLUDED.RelatedProductIds,
            Images = EXCLUDED.Images;
        """;

    public sealed record Parameters(
        Guid Id,
        string Sku,
        string Name,
        decimal PriceAmount,
        string PriceCurrency,
        string? Version,
        Guid[] CategoryIds,
        Guid[] RelatedProductIds,
        JsonbParameter Images
    );
}

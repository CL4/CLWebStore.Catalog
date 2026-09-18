using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

public static class PostgresTestDataSeeder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task SeedAsync(string connectionString, IEnumerable<ProductFixture> products)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var product in products)
        {
            var imagesJson = JsonSerializer.Serialize(product.Images, SerializerOptions);

            await connection.ExecuteAsync(
                """
                INSERT INTO read_schema.Products (
                    id,
                    sku,
                    name,
                    priceamount,
                    pricecurrency,
                    categoryids,
                    relatedproductids,
                    images
                )
                VALUES (
                    @Id,
                    @Sku,
                    @Name,
                    @PriceAmount,
                    @PriceCurrency,
                    @CategoryIds,
                    @RelatedProductIds,
                    @Images
                )
                ON CONFLICT (id)
                DO UPDATE SET
                    sku = EXCLUDED.sku,
                    name = EXCLUDED.name,
                    priceamount = EXCLUDED.priceamount,
                    pricecurrency = EXCLUDED.pricecurrency,
                    categoryids = EXCLUDED.categoryids,
                    relatedproductids = EXCLUDED.relatedproductids,
                    images = EXCLUDED.images
                """,
                new
                {
                    Id = product.Id,
                    Sku = product.Sku,
                    Name = product.Name,
                    PriceAmount = product.PriceAmount,
                    PriceCurrency = product.PriceCurrency,
                    CategoryIds = product.CategoryIds ?? [],
                    RelatedProductIds = product.RelatedProductIds ?? [],
                    Images = new JsonbParameter(imagesJson)
                });
        }
    }

    private sealed class JsonbParameter : SqlMapper.ICustomQueryParameter
    {
        private readonly string _json;

        public JsonbParameter(string json)
        {
            _json = json;
        }

        public void AddParameter(IDbCommand command, string name)
        {
            var parameterName = name.TrimStart('@', ':');
            var parameter = new NpgsqlParameter(parameterName, NpgsqlDbType.Jsonb)
            {
                Value = _json,
            };

            command.Parameters.Add(parameter);
        }
    }
}

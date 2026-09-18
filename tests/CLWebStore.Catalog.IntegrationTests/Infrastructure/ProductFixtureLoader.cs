using System.Text.Json;
using System.Text.Json.Serialization;

namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

public static class ProductFixtureLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static async Task<IReadOnlyList<ProductFixture>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Data", "Product_Data.json");

        if (!File.Exists(fixturePath))
        {
            throw new FileNotFoundException("Product fixture was not found in the test output directory.", fixturePath);
        }

        await using var stream = File.OpenRead(fixturePath);
        var products = await JsonSerializer.DeserializeAsync<List<ProductFixture>>(stream, SerializerOptions, cancellationToken);

        return products ?? [];
    }
}

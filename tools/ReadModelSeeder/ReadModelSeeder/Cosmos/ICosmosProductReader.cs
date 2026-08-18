using ReadModelSeeder.Models;

namespace ReadModelSeeder.Cosmos;

public interface ICosmosProductReader
{
    Task<IReadOnlyList<ProductDocument>> GetProductsAsync(CancellationToken cancellationToken);
}

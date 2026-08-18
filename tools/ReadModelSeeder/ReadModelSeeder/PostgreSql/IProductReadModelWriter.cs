using ReadModelSeeder.Models;

namespace ReadModelSeeder.PostgreSql;

public interface IProductReadModelWriter
{
    Task UpsertAsync(ProductDocument product, CancellationToken cancellationToken);
}

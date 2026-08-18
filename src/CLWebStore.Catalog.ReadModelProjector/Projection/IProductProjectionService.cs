using CLWebStore.Catalog.ReadModelProjector.Models;

namespace CLWebStore.Catalog.ReadModelProjector.Projection;

public interface IProductProjectionService
{
    public Task UpsertAsync(ProductDomainEvent productEvent, CancellationToken cancellationToken);
}

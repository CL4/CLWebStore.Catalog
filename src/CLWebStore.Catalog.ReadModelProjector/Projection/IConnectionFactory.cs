using System.Data.Common;

namespace CLWebStore.Catalog.ReadModelProjector.Projection;

public interface IConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}

using Npgsql;
using System.Data.Common;

namespace CLWebStore.Catalog.ReadModelProjector.Projection;

internal sealed class NpgsqlConnectionFactory : IConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        // Awaiting the call unpacks the NpgsqlConnection, casts it to DbConnection, 
        // and safely wraps it in the new ValueTask<DbConnection> return type.
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }
}

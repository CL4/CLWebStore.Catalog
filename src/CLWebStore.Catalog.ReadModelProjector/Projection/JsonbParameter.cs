using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace CLWebStore.Catalog.ReadModelProjector.Projection;

public sealed class JsonbParameter : SqlMapper.ICustomQueryParameter
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
            Value = _json
        };

        command.Parameters.Add(parameter);
    }
}

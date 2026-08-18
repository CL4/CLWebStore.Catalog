using Dapper;
using Npgsql;
using System.Data;

namespace ReadModelSeeder.PostgreSql;

public sealed class JsonbParameter : SqlMapper.ICustomQueryParameter
{
    private readonly string _json;

    public JsonbParameter(string json)
    {
        _json = json;
    }

    public void AddParameter(IDbCommand command, string name)
    {
        // Clean up the parameter name if Dapper sends it with a leading '@' or ':'
        var cleanName = name.StartsWith("@") || name.StartsWith(":")
            ? name[1..]
            : name;

        var parameter = new NpgsqlParameter(cleanName, NpgsqlTypes.NpgsqlDbType.Jsonb)
        {
            // Safely pass DBNull if the string happens to be null
            Value = _json ?? (object)DBNull.Value
        };

        command.Parameters.Add(parameter);
    }
}

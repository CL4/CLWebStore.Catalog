using Dapper;
using Npgsql;
using System.Data;
using System.Text.Json;

public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = JsonSerializer.Serialize(value);
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb;
        }
    }

    public override T? Parse(object value)
    {
        if (value is string json)
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        return default;
    }
}

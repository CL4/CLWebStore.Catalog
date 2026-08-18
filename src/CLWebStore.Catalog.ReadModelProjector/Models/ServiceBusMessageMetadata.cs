using Microsoft.Azure.Functions.Worker;

namespace CLWebStore.Catalog.ReadModelProjector.Models;

internal sealed record ServiceBusMessageMetadata(
    string? MessageId,
    string? CorrelationId,
    int? DeliveryCount,
    DateTimeOffset? EnqueuedTime)
{
    public static ServiceBusMessageMetadata From(FunctionContext context)
    {
        var bindingData = context.BindingContext.BindingData;

        return new ServiceBusMessageMetadata(
            GetString(bindingData, "MessageId"),
            GetString(bindingData, "CorrelationId"),
            GetInt32(bindingData, "DeliveryCount"),
            GetDateTimeOffset(bindingData, "EnqueuedTimeUtc") ?? GetDateTimeOffset(bindingData, "EnqueuedTime"));
    }

    private static object? GetValue(IReadOnlyDictionary<string, object?> bindingData, string key)
    {
        return bindingData.TryGetValue(key, out var value)
            ? value
            : bindingData.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
    }

    private static string? GetString(IReadOnlyDictionary<string, object?> bindingData, string key)
    {
        var value = GetValue(bindingData, key);

        return value switch
        {
            null => null,
            string stringValue when string.IsNullOrWhiteSpace(stringValue) => null,
            string stringValue => stringValue,
            _ => value.ToString()
        };
    }

    private static int? GetInt32(IReadOnlyDictionary<string, object?> bindingData, string key)
    {
        var value = GetValue(bindingData, key);

        return value switch
        {
            int intValue => intValue,
            long longValue when longValue is >= int.MinValue and <= int.MaxValue => (int)longValue,
            string stringValue when int.TryParse(stringValue, out var intValue) => intValue,
            _ => null
        };
    }

    private static DateTimeOffset? GetDateTimeOffset(IReadOnlyDictionary<string, object?> bindingData, string key)
    {
        var value = GetValue(bindingData, key);

        return value switch
        {
            DateTimeOffset dateTimeOffset => dateTimeOffset,
            DateTime dateTime => new DateTimeOffset(dateTime),
            string stringValue when DateTimeOffset.TryParse(stringValue, out var dateTimeOffset) => dateTimeOffset,
            _ => null
        };
    }
}

using Azure;
using Azure.Data.Tables;

namespace CLWebStore.Catalog.OutboxProcessor.Models;

public class DeadLetterEntity : ITableEntity
{
    public string PartitionKey
    {
        get; set;
    }
    public string RowKey
    {
        get; set;
    }

    public string ErrorMessage
    {
        get; set;
    }
    public string StackTrace
    {
        get; set;
    }
    public string OriginalData
    {
        get; set;
    }
    public DateTimeOffset? Timestamp
    {
        get; set;
    }
    public ETag ETag
    {
        get; set;
    }

    public DeadLetterEntity()
    {
    } // Required for ITableEntity

    public DeadLetterEntity(string messageId, string messageType)
    {
        PartitionKey = messageType ?? "UnknownType";
        RowKey = messageId ?? Guid.NewGuid().ToString();
    }
}
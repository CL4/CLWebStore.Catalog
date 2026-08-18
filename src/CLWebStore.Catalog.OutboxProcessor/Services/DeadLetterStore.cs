using Azure.Data.Tables;
using CLWebStore.Catalog.OutboxProcessor.Models;
using System.Text.Json;

namespace CLWebStore.Catalog.OutboxProcessor.Services;

public class DeadLetterStore : IDeadLetterStore
{
    private readonly TableClient _tableClient;

    public DeadLetterStore(TableServiceClient tableServiceClient)
    {
        // TableServiceClient is injected via DI
        _tableClient = tableServiceClient.GetTableClient("OutboxDeadLetter");
        _tableClient.CreateIfNotExists();
    }

    public async Task CaptureAsync(object outboxItem, Exception ex, string messageId, string messageType)
    {
        var entity = new DeadLetterEntity(messageId, messageType)
        {
            ErrorMessage = ex.Message,
            StackTrace = ex.StackTrace,
            OriginalData = JsonSerializer.Serialize(outboxItem)
        };

        await _tableClient.AddEntityAsync(entity);
    }
}

using Microsoft.Extensions.Logging;
using System.Net;

namespace CLWebStore.Catalog.Infrastructure.Observability.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Product with ID {ProductId} was not found in the database.")]
    public static partial void LogProductNotFound(this ILogger logger, Guid productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "An unexpected error occurred while fetching product {ProductId}")]
    public static partial void LogProductFetchError(this ILogger logger, Exception ex, Guid productId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to atomically save Product {ProductId} and its Outbox events. Cosmos DB Status Code: {StatusCode}")]
    public static partial void LogProductSaveWarning(this ILogger logger, Guid productId, HttpStatusCode statusCode);

    [LoggerMessage(Level = LogLevel.Error, Message = "An exception occurred while executing the TransactionalBatch for Product {ProductId}")]
    public static partial void LogProductSaveError(this ILogger logger, Exception ex, Guid productId);
}

using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Infrastructure.Observability.Logging;
using CLWebStore.Catalog.Infrastructure.Observability.Tracing;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Documents;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos.Mappings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace CLWebStore.Catalog.Infrastructure.Repositories;

public class ProductRepository(ICosmosClientFactory factory, ILogger<ProductRepository> logger) : IProductRepository
{
    private readonly Container _container = factory.GetCatalogContainer();
    private readonly ILogger<ProductRepository> _logger = logger;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var partitionKey = new PartitionKey(id.ToString());

            ItemResponse<ProductDocument> response = await _container.ReadItemAsync<ProductDocument>(
                id.ToString(),
                partitionKey,
                cancellationToken: cancellationToken);

            return ProductMapper.ToDomain(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // This is an expected domain scenario (Product not found), so we generally don't log it as an error.
            // You could log it as Trace/Debug if you want deep diagnostics.
            _logger.LogProductNotFound(id);
            return null;
        }
        catch (Exception ex)
        {
            // Unexpected errors during read should be logged
            _logger.LogProductFetchError(ex, id);
            throw;
        }
    }

    public async Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Source.StartActivity("ProductRepository.SaveAsync");

        activity?.SetTag("product.id", product.Id.ToString());
        activity?.SetTag("outbox.event_count", product.DomainEvents.Count);

        var productDoc = ProductMapper.ToDocument(product);
        var partitionKey = new PartitionKey(product.Id.ToString());

        var batch = _container.CreateTransactionalBatch(partitionKey);

        if (string.IsNullOrEmpty(productDoc.Etag))
        {
            batch.CreateItem(productDoc);
            activity?.SetTag("db.operation", "Create");
        }
        else
        {
            batch.ReplaceItem(productDoc.Id, productDoc, new TransactionalBatchItemRequestOptions { IfMatchEtag = productDoc.Etag });
            activity?.SetTag("db.operation", "Update");
        }

        foreach (var domainEvent in product.DomainEvents)
        {
            var outboxDoc = ProductMapper.ToOutboxDocument(domainEvent, product.Id);
            batch.CreateItem(outboxDoc);
        }

        try
        {
            using var batchResponse = await batch.ExecuteAsync(cancellationToken);

            if (!batchResponse.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Error, $"Batch failed with status: {batchResponse.StatusCode}");

                // Log the semantic warning/error with the status code
                _logger.LogProductSaveWarning(product.Id, batchResponse.StatusCode);

                throw new Exception($"Failed to save Product and Outbox events atomically. Status: {batchResponse.StatusCode}");
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            product.ClearDomainEvents();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            // Log the actual exception and stack trace
            _logger.LogProductSaveError(ex, product.Id);

            throw;
        }
    }
}

using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Domain.ValueObjects;
using MediatR;

namespace CLWebStore.Catalog.Application.Commands.V1.CreateProduct;

public class CreateProductHandler(IProductRepository repository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository = repository;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var sku = new Sku(request.Sku);
        var name = new ProductName(request.Name);
        var price = new Money(request.PriceAmount, request.PriceCurrency);

        // Capture a single, stable timestamp for the entire transaction
        var occurredOn = timeProvider.GetUtcNow();

        // 1. Create the base product
        var product = Product.Create(sku, name, price, DateTimeOffset.UtcNow);

        // 2. Hydrate optional collections using Domain methods
        if (request.CategoryIds?.Any() == true)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                product.AddCategory(categoryId, occurredOn);
            }
        }

        if (request.RelatedProductIds?.Any() == true)
        {
            foreach (var relatedProductId in request.RelatedProductIds)
            {
                product.AddRelatedProduct(relatedProductId, occurredOn);
            }
        }

        if (request.Images?.Any() == true)
        {
            foreach (var image in request.Images)
            {
                product.AddImage(image.Url, image.AltText, image.IsPrimary, occurredOn);
            }
        }

        // 3. Persist the fully constructed aggregate
        await _repository.SaveAsync(product, cancellationToken);

        return product.Id;
    }
}
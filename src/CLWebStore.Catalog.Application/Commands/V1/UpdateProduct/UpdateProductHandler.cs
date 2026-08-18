using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Domain.ValueObjects;
using MediatR;

namespace CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;

public class UpdateProductHandler(IProductRepository repository,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repository = repository;

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        var name = new ProductName(request.Name);
        var price = new Money(request.PriceAmount, request.PriceCurrency);

        // Capture a single, stable timestamp for the entire transaction
        var occurredOn = timeProvider.GetUtcNow();

        // 1. Update primitive aggregate properties
        product.UpdateDetails(name, price, occurredOn);

        // 2. Sync Categories (Compute Difference)
        if (request.CategoryIds != null)
        {
            var existingCategories = product.CategoryIds.ToList();
            var categoriesToRemove = existingCategories.Except(request.CategoryIds);
            var categoriesToAdd = request.CategoryIds.Except(existingCategories);

            foreach (var categoryId in categoriesToRemove) product.RemoveCategory(categoryId, occurredOn);
            foreach (var categoryId in categoriesToAdd) product.AddCategory(categoryId, occurredOn);
        }

        // 3. Sync Related Products (Compute Difference)
        if (request.RelatedProductIds != null)
        {
            var existingRelated = product.RelatedProductIds.ToList();
            var relatedToRemove = existingRelated.Except(request.RelatedProductIds);
            var relatedToAdd = request.RelatedProductIds.Except(existingRelated);

            foreach (var relatedId in relatedToRemove) product.RemoveRelatedProduct(relatedId, occurredOn);
            foreach (var relatedId in relatedToAdd) product.AddRelatedProduct(relatedId, occurredOn);
        }

        // 4. Sync Images (Smart Update Strategy)
        if (request.Images != null)
        {
            var existingImageIds = product.Images.Select(i => i.Id).ToList();
            var incomingImageIds = request.Images.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();

            // A. Remove images that are in the DB but NOT in the incoming request
            var imagesToRemove = existingImageIds.Except(incomingImageIds);
            foreach (var imageId in imagesToRemove)
            {
                product.RemoveImage(imageId, occurredOn);
            }

            // B. Add or Update incoming images
            foreach (var imageDto in request.Images)
            {
                if (imageDto.Id.HasValue && existingImageIds.Contains(imageDto.Id.Value))
                {
                    // Update existing image
                    product.UpdateImage(imageDto.Id.Value, imageDto.Url, imageDto.AltText, imageDto.IsPrimary, occurredOn);
                }
                else
                {
                    // Add new image (No ID provided, or a fake/invalid ID was provided)
                    product.AddImage(imageDto.Url, imageDto.AltText, imageDto.IsPrimary, occurredOn);
                }
            }
        }

        await _repository.SaveAsync(product, cancellationToken);
    }
}

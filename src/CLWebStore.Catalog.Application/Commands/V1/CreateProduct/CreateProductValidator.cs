using FluentValidation;

namespace CLWebStore.Catalog.Application.Commands.V1.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PriceAmount).GreaterThan(0);
        RuleFor(x => x.PriceCurrency).NotEmpty().Length(3);

        // Optional list validations
        RuleForEach(x => x.CategoryIds)
            .NotEmpty()
            .When(x => x.CategoryIds != null);

        RuleForEach(x => x.RelatedProductIds)
            .NotEmpty()
            .When(x => x.RelatedProductIds != null);

        RuleForEach(x => x.Images)
            .SetValidator(new CreateProductImageDtoValidator())
            .When(x => x.Images != null);
    }
}

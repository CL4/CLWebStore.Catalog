using FluentValidation;

namespace CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PriceAmount).GreaterThan(0);
        RuleFor(x => x.PriceCurrency).NotEmpty().Length(3);
    }
}

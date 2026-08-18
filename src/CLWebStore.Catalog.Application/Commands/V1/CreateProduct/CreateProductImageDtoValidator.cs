using CLWebStore.Catalog.Application.DTOs.V1;
using FluentValidation;

namespace CLWebStore.Catalog.Application.Commands.V1.CreateProduct;

public class CreateProductImageDtoValidator : AbstractValidator<CreateProductImageDto>
{
    public CreateProductImageDtoValidator()
    {
        RuleFor(x => x.Url).NotEmpty().WithMessage("Image URL is required.");
    }
}

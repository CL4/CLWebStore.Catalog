using CLWebStore.Catalog.Application.Commands.V1.CreateProduct;
using CLWebStore.Catalog.Application.DTOs.V1;

namespace CLWebStore.Catalog.UnitTests.Application.Commands.V1.CreateProduct;

public class CreateProductImageDtoValidatorTests
{
    [Fact]
    public void Validate_ValidImage_Passes()
    {
        var validator = new CreateProductImageDtoValidator();

        var dto = new CreateProductImageDto("http://example.com/img.jpg", "alt", true);

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidUrl_Fails(string url)
    {
        var validator = new CreateProductImageDtoValidator();

        var dto = new CreateProductImageDto(url, "alt", false);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Url");
    }
}

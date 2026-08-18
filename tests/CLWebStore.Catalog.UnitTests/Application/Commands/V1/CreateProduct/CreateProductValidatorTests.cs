using CLWebStore.Catalog.Application.Commands.V1.CreateProduct;

namespace CLWebStore.Catalog.UnitTests.Application.Commands.V1.CreateProduct;

public class CreateProductValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var validator = new CreateProductValidator();

        var cmd = new CreateProductCommand(
            Sku: "SKU-1",
            Name: "Name 1",
            PriceAmount: 1.23m,
            PriceCurrency: "USD",
            CategoryIds: null,
            RelatedProductIds: null,
            Images: null
        );

        var result = validator.Validate(cmd);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidSku_Fails(string sku)
    {
        var validator = new CreateProductValidator();

        var cmd = new CreateProductCommand(
            Sku: sku,
            Name: "Name",
            PriceAmount: 1m,
            PriceCurrency: "USD",
            CategoryIds: null,
            RelatedProductIds: null,
            Images: null
        );

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Sku");
    }

    [Fact]
    public void Validate_InvalidPrice_Fails()
    {
        var validator = new CreateProductValidator();

        var cmd = new CreateProductCommand(
            Sku: "SKU",
            Name: "Name",
            PriceAmount: 0m,
            PriceCurrency: "USD",
            CategoryIds: null,
            RelatedProductIds: null,
            Images: null
        );

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PriceAmount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidName_Fails(string name)
    {
        var validator = new CreateProductValidator();

        var cmd = new CreateProductCommand(
            Sku: "SKU",
            Name: name,
            PriceAmount: 1m,
            PriceCurrency: "USD",
            CategoryIds: null,
            RelatedProductIds: null,
            Images: null
        );

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }
}

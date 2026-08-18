using CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;

namespace CLWebStore.Catalog.UnitTests.Application.Commands.V1.UpdateProduct;

public class UpdateProductValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var validator = new UpdateProductValidator();

        var cmd = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Name",
            PriceAmount = 1m,
            PriceCurrency = "USD"
        };

        var result = validator.Validate(cmd);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_Fails()
    {
        var validator = new UpdateProductValidator();

        var cmd = new UpdateProductCommand
        {
            Id = Guid.Empty,
            Name = "Name",
            PriceAmount = 1m,
            PriceCurrency = "USD"
        };

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void Validate_InvalidPrice_Fails()
    {
        var validator = new UpdateProductValidator();

        var cmd = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Name",
            PriceAmount = 0m,
            PriceCurrency = "USD"
        };

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PriceAmount");
    }

    [Fact]
    public void Validate_InvalidCurrency_Fails()
    {
        var validator = new UpdateProductValidator();

        var cmd = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Name",
            PriceAmount = 1m,
            PriceCurrency = "US" // invalid length
        };

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PriceCurrency");
    }
}

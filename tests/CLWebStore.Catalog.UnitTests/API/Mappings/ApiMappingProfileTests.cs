using AutoMapper;
using CLWebStore.Catalog.API.Contracts.V1.Requests;
using CLWebStore.Catalog.API.Mappings;
using CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;
using Microsoft.Extensions.Logging.Abstractions; // <-- Required for NullLoggerFactory

namespace CLWebStore.Catalog.UnitTests.API.Mappings;

public class ApiMappingProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        // Pass NullLoggerFactory.Instance as the second parameter
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile(new ApiMappingProfile()),
            NullLoggerFactory.Instance);

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void UpdateProductRequest_MapsTo_UpdateProductCommand_WithImageIdsPreserved()
    {
        // Pass NullLoggerFactory.Instance as the second parameter
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile(new ApiMappingProfile()),
            NullLoggerFactory.Instance);

        var mapper = config.CreateMapper();

        var imageId = System.Guid.NewGuid();
        var request = new UpdateProductRequest(
            Name: "Name",
            PriceAmount: 1.23m,
            PriceCurrency: "USD",
            CategoryIds: null,
            RelatedProductIds: null,
            Images: new List<UpdateProductImageRequest>
            {
                new(imageId, "http://x", "alt", true),
                new(null, "http://y", "alt2", false)
            }
        );

        var command = mapper.Map<UpdateProductCommand>(request);

        Assert.Equal(request.Name, command.Name);
        Assert.Equal(request.PriceAmount, command.PriceAmount);
        Assert.Equal(request.PriceCurrency, command.PriceCurrency);
        Assert.NotNull(command.Images);
        Assert.Equal(2, command.Images!.Count);
        Assert.Equal(imageId, command.Images![0].Id);
        Assert.Null(command.Images![1].Id);
    }
}

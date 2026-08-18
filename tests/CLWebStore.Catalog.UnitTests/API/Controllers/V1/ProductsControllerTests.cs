using AutoMapper;
using CLWebStore.Catalog.API.Contracts.V1.Requests;
using CLWebStore.Catalog.API.Contracts.V1.Responses;
using CLWebStore.Catalog.Application.Commands.V1.CreateProduct;
using CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Application.Queries.V1.GetProductById;
using CLWebStore.Catalog.Application.Queries.V1.GetProductsByCategory;
using CLWebStore.Catalog.Application.Queries.V1.GetProductsBySku;
using CLWebStore.Catalog.Application.Queries.V1.GetRelatedProducts;
using CLWebStore.Catalog.Application.Queries.V1.SearchProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CLWebStore.Catalog.UnitTests.API.Controllers.V1;

public class ProductsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CLWebStore.Catalog.API.Controllers.V1.ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();

        _controller = new CLWebStore.Catalog.API.Controllers.V1.ProductsController(_mockMediator.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateProduct_MapsCommand_And_ReturnsCreatedAtAction()
    {
        var request = new CreateProductRequest("SKU-1", "Name", 1.23m, "USD", null, null, null);
        var productId = Guid.NewGuid();

        var command = new CreateProductCommand(request.Sku, request.Name, request.PriceAmount, request.PriceCurrency, request.CategoryIds, request.RelatedProductIds, null);

        _mockMapper.Setup(m => m.Map<CreateProductCommand>(It.IsAny<CreateProductRequest>())).Returns(command);
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(productId);

        var result = await _controller.CreateProduct(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetProduct), created.ActionName);
        var value = Assert.IsType<ProductCreatedResponse>(created.Value);
        Assert.Equal(productId, value.Id);
    }

    [Fact]
    public async Task UpdateProduct_MapsCommand_WithRouteId_And_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var request = new UpdateProductRequest(
            Name: "Updated",
            PriceAmount: 2.22m,
            PriceCurrency: "USD",
            CategoryIds: null,
            RelatedProductIds: null,
            Images: new List<CLWebStore.Catalog.API.Contracts.V1.Requests.UpdateProductImageRequest>
            {
                new CLWebStore.Catalog.API.Contracts.V1.Requests.UpdateProductImageRequest(imageId, "http://x", "alt", true)
            }
        );

        var mapped = new UpdateProductCommand
        {
            Name = request.Name,
            PriceAmount = request.PriceAmount,
            PriceCurrency = request.PriceCurrency,
            Images = new List<UpdateProductImageDto> { new UpdateProductImageDto(imageId, "http://x", "alt", true) }
        };

        _mockMapper.Setup(m => m.Map<UpdateProductCommand>(It.IsAny<UpdateProductRequest>())).Returns(mapped);

        UpdateProductCommand? sent = null;

        // Fix: Use Task.CompletedTask and match the callback signature exactly
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateProductCommand, CancellationToken>((c, ct) => sent = c)
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateProduct(id, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.NotNull(sent);
        Assert.Equal(id, sent!.Id);
        Assert.Single(sent.Images!);
        Assert.Equal(imageId, sent.Images![0].Id);
    }

    [Fact]
    public async Task GetProduct_ReturnsOk_WithProductDto()
    {
        var id = Guid.NewGuid();
        var dto = new ProductDto { Id = id, Sku = "SKU-1", Name = "P", PriceAmount = 1m, PriceCurrency = "USD" };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _controller.GetProduct(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetProductsBySku_Found_ReturnsOk_WithSingleProduct()
    {
        var sku = "SKU-A";
        var dto = new ProductDto { Id = Guid.NewGuid(), Sku = sku, Name = "A", PriceAmount = 1m, PriceCurrency = "USD" };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetProductsBySkuQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { dto });

        var result = await _controller.GetProductsBySku(sku, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ProductDto>(ok.Value);
        Assert.Equal(sku, value.Sku);
    }

    [Fact]
    public async Task GetProductsBySku_NotFound_ReturnsNotFound()
    {
        var sku = "SKU-X";

        _mockMediator.Setup(m => m.Send(It.IsAny<GetProductsBySkuQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<ProductDto>());

        var result = await _controller.GetProductsBySku(sku, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsOk_WithCollection()
    {
        var categoryId = Guid.NewGuid();
        var list = new List<ProductDto> { new ProductDto { Id = Guid.NewGuid(), Sku = "C1", Name = "C1" } };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetProductsByCategoryQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _controller.GetProductsByCategory(categoryId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }

    [Fact]
    public async Task GetRelatedProducts_ReturnsOk_WithCollection()
    {
        var id = Guid.NewGuid();
        var list = new List<ProductDto> { new ProductDto { Id = Guid.NewGuid(), Sku = "R1", Name = "R1" } };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetRelatedProductsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _controller.GetRelatedProducts(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }

    [Fact]
    public async Task SearchProducts_ReturnsOk_WithCollection()
    {
        var term = "widget";
        var list = new List<ProductDto> { new ProductDto { Id = Guid.NewGuid(), Sku = "W1", Name = "Widget" } };

        _mockMediator.Setup(m => m.Send(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _controller.SearchProducts(term, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }
}

using Asp.Versioning;
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

namespace CLWebStore.Catalog.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController(IMediator mediator, IMapper mapper) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMapper _mapper = mapper;

    #region POST Endpoints

    [HttpPost]
    [ProducesResponseType(typeof(ProductCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateProductCommand>(request);
        var productId = await _mediator.Send(command, cancellationToken);

        var response = new ProductCreatedResponse(productId);

        return CreatedAtAction(
            actionName: nameof(GetProduct),
            routeValues: new { version = "1.0", id = productId },
            value: response);
    }

    #endregion

    #region PUT Endpoints

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateProductCommand>(request) with { Id = id };

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    #endregion

    #region GET Endpoints

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var product = await _mediator.Send(query, cancellationToken);

        return Ok(product);
    }

    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductsBySku(string sku, CancellationToken cancellationToken)
    {
        var query = new GetProductsBySkuQuery(new[] { sku });
        var products = await _mediator.Send(query, cancellationToken); var product = products.FirstOrDefault();

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var query = new GetProductsByCategoryQuery(categoryId); var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatedProducts(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRelatedProductsQuery(id); var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm, CancellationToken cancellationToken)
    {
        var query = new SearchProductsQuery(searchTerm); var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    #endregion
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Products;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ProductDto>> Search([FromQuery] ProductSearchRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new GetProductsQuery(request), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return product is null ? NotFound() : product;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public Task<ProductDto> Create(UpsertProductRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new CreateProductCommand(request), cancellationToken);

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new UpdateProductCommand(id, request), cancellationToken);
        return product is null ? NotFound() : product;
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await mediator.Send(new DeleteProductCommand(id), cancellationToken) ? NoContent() : NotFound();

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/images")]
    public async Task<ActionResult<ProductImageDto>> UploadImage(Guid id, IFormFile file, [FromForm] bool isPrimary, CancellationToken cancellationToken)
    {
        var image = await mediator.Send(new UploadProductImageCommand(id, file, isPrimary), cancellationToken);
        return image is null ? NotFound() : image;
    }
}

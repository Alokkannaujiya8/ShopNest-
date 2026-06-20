using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Products;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> Get(CancellationToken cancellationToken) =>
        mediator.Send(new GetCategoriesQuery(), cancellationToken);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public Task<CategoryDto> Create(UpsertCategoryRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new CreateCategoryCommand(request), cancellationToken);
}

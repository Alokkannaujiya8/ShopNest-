using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Orders;

namespace ShopNest.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public sealed class AdminController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard")]
    public Task<DashboardStatsDto> Dashboard(CancellationToken cancellationToken) =>
        mediator.Send(new GetDashboardStatsQuery(), cancellationToken);

    [HttpPatch("inventory/{productId:guid}")]
    public async Task<IActionResult> UpdateInventory(Guid productId, InventoryUpdateRequest request, CancellationToken cancellationToken) =>
        await mediator.Send(new UpdateInventoryCommand(productId, request), cancellationToken) ? NoContent() : NotFound();

    [HttpGet("orders")]
    public Task<PagedResult<OrderDto>> Orders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        mediator.Send(new GetAllOrdersQuery(page, pageSize), cancellationToken);
}

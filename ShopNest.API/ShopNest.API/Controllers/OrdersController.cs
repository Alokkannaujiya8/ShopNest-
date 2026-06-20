using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Orders;

namespace ShopNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    public Task<PagedResult<OrderDto>> MyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        mediator.Send(new GetMyOrdersQuery(User.UserId(), page, pageSize), cancellationToken);

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<PagedResult<OrderDto>> All([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        mediator.Send(new GetAllOrdersQuery(page, pageSize), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var order = await mediator.Send(new GetOrderByIdQuery(id, isAdmin ? null : User.UserId()), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new UpdateOrderStatusCommand(id, request), cancellationToken);
        return order is null ? NotFound() : order;
    }
}

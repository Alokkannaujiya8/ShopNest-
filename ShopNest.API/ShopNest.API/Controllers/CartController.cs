using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Cart;
using ShopNest.Application.Features.Orders;

namespace ShopNest.API.Controllers;

[Authorize(Roles = "Customer,Admin")]
[ApiController]
[Route("api/cart")]
public sealed class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<CartDto> Get(CancellationToken cancellationToken) =>
        mediator.Send(new GetCartQuery(User.UserId()), cancellationToken);

    [HttpPost("items")]
    public Task<CartDto> Add(AddCartItemRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new AddToCartCommand(User.UserId(), request), cancellationToken);

    [HttpPut("items/{itemId:guid}")]
    public Task<CartDto> Update(Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new UpdateCartItemCommand(User.UserId(), itemId, request), cancellationToken);

    [HttpDelete("items/{itemId:guid}")]
    public Task<CartDto> Remove(Guid itemId, CancellationToken cancellationToken) =>
        mediator.Send(new RemoveCartItemCommand(User.UserId(), itemId), cancellationToken);

    [HttpPost("checkout")]
    public Task<OrderDto> Checkout(CheckoutRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new CheckoutCommand(User.UserId(), request), cancellationToken);
}

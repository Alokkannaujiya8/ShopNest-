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

    [HttpPut("items/{itemId:guid}/increase")]
    public Task<CartDto> Increase(Guid itemId, CancellationToken cancellationToken) =>
        mediator.Send(new IncreaseCartItemQuantityCommand(User.UserId(), itemId), cancellationToken);

    [HttpPut("items/{itemId:guid}/decrease")]
    public Task<CartDto> Decrease(Guid itemId, CancellationToken cancellationToken) =>
        mediator.Send(new DecreaseCartItemQuantityCommand(User.UserId(), itemId), cancellationToken);

    [HttpDelete("clear")]
    public Task<CartDto> Clear(CancellationToken cancellationToken) =>
        mediator.Send(new ClearCartCommand(User.UserId()), cancellationToken);

    [HttpPost("coupon/apply")]
    public Task<CartDto> ApplyCoupon([FromBody] string couponCode, CancellationToken cancellationToken) =>
        mediator.Send(new ApplyCouponCommand(User.UserId(), couponCode), cancellationToken);

    [HttpDelete("coupon/remove")]
    public Task<CartDto> RemoveCoupon(CancellationToken cancellationToken) =>
        mediator.Send(new RemoveCouponCommand(User.UserId()), cancellationToken);

    [HttpPost("items/move-from-wishlist/{productId:guid}")]
    public Task<CartDto> MoveFromWishlist(Guid productId, CancellationToken cancellationToken) =>
        mediator.Send(new MoveWishlistItemToCartCommand(User.UserId(), productId), cancellationToken);

    [HttpPost("checkout")]
    public Task<OrderDto> Checkout(CheckoutRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new CheckoutCommand(User.UserId(), request), cancellationToken);
}

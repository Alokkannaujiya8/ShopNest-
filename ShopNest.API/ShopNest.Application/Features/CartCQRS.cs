using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Cart;

public sealed record GetCartQuery(Guid UserId) : IRequest<CartDto>;
public sealed record AddToCartCommand(Guid UserId, AddCartItemRequest Request) : IRequest<CartDto>;
public sealed record UpdateCartItemCommand(Guid UserId, Guid CartItemId, UpdateCartItemRequest Request) : IRequest<CartDto>;
public sealed record IncreaseCartItemQuantityCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;
public sealed record DecreaseCartItemQuantityCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;
public sealed record RemoveCartItemCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;
public sealed record ClearCartCommand(Guid UserId) : IRequest<CartDto>;
public sealed record ApplyCouponCommand(Guid UserId, string CouponCode) : IRequest<CartDto>;
public sealed record RemoveCouponCommand(Guid UserId) : IRequest<CartDto>;
public sealed record MoveWishlistItemToCartCommand(Guid UserId, Guid ProductId) : IRequest<CartDto>;

public sealed class CartHandlers(ICartOrderService carts) :
    IRequestHandler<GetCartQuery, CartDto>,
    IRequestHandler<AddToCartCommand, CartDto>,
    IRequestHandler<UpdateCartItemCommand, CartDto>,
    IRequestHandler<IncreaseCartItemQuantityCommand, CartDto>,
    IRequestHandler<DecreaseCartItemQuantityCommand, CartDto>,
    IRequestHandler<RemoveCartItemCommand, CartDto>,
    IRequestHandler<ClearCartCommand, CartDto>,
    IRequestHandler<ApplyCouponCommand, CartDto>,
    IRequestHandler<RemoveCouponCommand, CartDto>,
    IRequestHandler<MoveWishlistItemToCartCommand, CartDto>
{
    public Task<CartDto> Handle(GetCartQuery query, CancellationToken cancellationToken) =>
        carts.GetCartAsync(query.UserId, cancellationToken);

    public Task<CartDto> Handle(AddToCartCommand command, CancellationToken cancellationToken) =>
        carts.AddToCartAsync(command.UserId, command.Request, cancellationToken);

    public Task<CartDto> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken) =>
        carts.UpdateCartItemAsync(command.UserId, command.CartItemId, command.Request, cancellationToken);

    public Task<CartDto> Handle(IncreaseCartItemQuantityCommand command, CancellationToken cancellationToken) =>
        carts.IncreaseCartItemQuantityAsync(command.UserId, command.CartItemId, cancellationToken);

    public Task<CartDto> Handle(DecreaseCartItemQuantityCommand command, CancellationToken cancellationToken) =>
        carts.DecreaseCartItemQuantityAsync(command.UserId, command.CartItemId, cancellationToken);

    public Task<CartDto> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken) =>
        carts.RemoveCartItemAsync(command.UserId, command.CartItemId, cancellationToken);

    public Task<CartDto> Handle(ClearCartCommand command, CancellationToken cancellationToken) =>
        carts.ClearCartAsync(command.UserId, cancellationToken);

    public Task<CartDto> Handle(ApplyCouponCommand command, CancellationToken cancellationToken) =>
        carts.ApplyCouponAsync(command.UserId, command.CouponCode, cancellationToken);

    public Task<CartDto> Handle(RemoveCouponCommand command, CancellationToken cancellationToken) =>
        carts.RemoveCouponAsync(command.UserId, cancellationToken);

    public Task<CartDto> Handle(MoveWishlistItemToCartCommand command, CancellationToken cancellationToken) =>
        carts.MoveWishlistItemToCartAsync(command.UserId, command.ProductId, cancellationToken);
}

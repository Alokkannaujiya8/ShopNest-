using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Cart;

public sealed record GetCartQuery(Guid UserId) : IRequest<CartDto>;
public sealed record AddToCartCommand(Guid UserId, AddCartItemRequest Request) : IRequest<CartDto>;
public sealed record UpdateCartItemCommand(Guid UserId, Guid CartItemId, UpdateCartItemRequest Request) : IRequest<CartDto>;
public sealed record RemoveCartItemCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;

public sealed class CartHandlers(ICartOrderService carts) :
    IRequestHandler<GetCartQuery, CartDto>,
    IRequestHandler<AddToCartCommand, CartDto>,
    IRequestHandler<UpdateCartItemCommand, CartDto>,
    IRequestHandler<RemoveCartItemCommand, CartDto>
{
    public Task<CartDto> Handle(GetCartQuery query, CancellationToken cancellationToken) =>
        carts.GetCartAsync(query.UserId, cancellationToken);

    public Task<CartDto> Handle(AddToCartCommand command, CancellationToken cancellationToken) =>
        carts.AddToCartAsync(command.UserId, command.Request, cancellationToken);

    public Task<CartDto> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken) =>
        carts.UpdateCartItemAsync(command.UserId, command.CartItemId, command.Request, cancellationToken);

    public Task<CartDto> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken) =>
        carts.RemoveCartItemAsync(command.UserId, command.CartItemId, cancellationToken);
}

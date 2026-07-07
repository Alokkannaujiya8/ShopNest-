using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

public sealed record GetWishlistQuery(Guid UserId, WishlistSearchRequest Request) : IRequest<PagedResult<WishlistItemDto>>;
public sealed record GetWishlistCountQuery(Guid UserId) : IRequest<int>;
public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<WishlistItemDto>;
public sealed record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest<bool>;
public sealed record ClearWishlistCommand(Guid UserId) : IRequest<bool>;
public sealed record MoveToCartCommand(Guid UserId, Guid ProductId) : IRequest<bool>;

public sealed class WishlistCQRSHandlers(IWishlistService wishlistService) :
    IRequestHandler<GetWishlistQuery, PagedResult<WishlistItemDto>>,
    IRequestHandler<GetWishlistCountQuery, int>,
    IRequestHandler<AddToWishlistCommand, WishlistItemDto>,
    IRequestHandler<RemoveFromWishlistCommand, bool>,
    IRequestHandler<ClearWishlistCommand, bool>,
    IRequestHandler<MoveToCartCommand, bool>
{
    public Task<PagedResult<WishlistItemDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken) =>
        wishlistService.GetWishlistAsync(request.UserId, request.Request, cancellationToken);

    public Task<int> Handle(GetWishlistCountQuery request, CancellationToken cancellationToken) =>
        wishlistService.GetWishlistCountAsync(request.UserId, cancellationToken);

    public Task<WishlistItemDto> Handle(AddToWishlistCommand request, CancellationToken cancellationToken) =>
        wishlistService.AddToWishlistAsync(request.UserId, request.ProductId, cancellationToken);

    public Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken) =>
        wishlistService.RemoveFromWishlistAsync(request.UserId, request.ProductId, cancellationToken);

    public Task<bool> Handle(ClearWishlistCommand request, CancellationToken cancellationToken) =>
        wishlistService.ClearWishlistAsync(request.UserId, cancellationToken);

    public Task<bool> Handle(MoveToCartCommand request, CancellationToken cancellationToken) =>
        wishlistService.MoveToCartAsync(request.UserId, request.ProductId, cancellationToken);
}

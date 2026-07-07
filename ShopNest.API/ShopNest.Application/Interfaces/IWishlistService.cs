using System;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IWishlistService
{
    Task<PagedResult<WishlistItemDto>> GetWishlistAsync(Guid userId, WishlistSearchRequest request, CancellationToken cancellationToken);
    Task<int> GetWishlistCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<WishlistItemDto> AddToWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<bool> ClearWishlistAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> MoveToCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
}

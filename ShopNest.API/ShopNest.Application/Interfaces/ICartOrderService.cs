using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface ICartOrderService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartDto> AddToCartAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken);
    Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken);
    Task<PagedResult<OrderDto>> GetOrdersAsync(Guid? userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderDto?> GetOrderAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken);
    Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken);
}

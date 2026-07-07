using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface ICartOrderService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartDto> AddToCartAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> IncreaseCartItemQuantityAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken);
    Task<CartDto> DecreaseCartItemQuantityAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken);
    Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken);
    Task<CartDto> ClearCartAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartDto> ApplyCouponAsync(Guid userId, string couponCode, CancellationToken cancellationToken);
    Task<CartDto> RemoveCouponAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartDto> MoveWishlistItemToCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken);
    Task<PagedResult<OrderDto>> GetOrdersAsync(Guid? userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderDto?> GetOrderAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken);
    Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken);
    Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, Guid? userId, CancellationToken cancellationToken);
    Task<OrderDto?> CancelOrderAsync(Guid orderId, Guid? userId, string reason, CancellationToken cancellationToken);
    Task<OrderDto?> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus, CancellationToken cancellationToken);
    Task<OrderDto?> RequestReturnAsync(Guid orderId, Guid userId, string reason, CancellationToken cancellationToken);
    Task<OrderDto?> RequestRefundAsync(Guid orderId, Guid userId, string reason, CancellationToken cancellationToken);
    Task<OrderDto?> AssignCourierAsync(Guid orderId, string courierPartner, string trackingNumber, CancellationToken cancellationToken);
    Task<List<OrderStatusHistoryDto>> GetOrderTimelineAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken);
    Task<List<OrderTrackingDto>> GetOrderTrackingAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken);
    Task<OrderDto?> AddTrackingUpdateAsync(Guid orderId, string status, string location, CancellationToken cancellationToken);
}

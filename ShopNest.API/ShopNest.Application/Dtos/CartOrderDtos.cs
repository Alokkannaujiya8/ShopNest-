using ShopNest.Domain.Enums;

namespace ShopNest.Application.Dtos;

public sealed record CartItemDto(
    Guid Id, 
    Guid ProductId, 
    string ProductName, 
    string ProductSku,
    string? BrandName,
    string CategoryName,
    string? ImageUrl,
    decimal UnitPrice, 
    decimal OriginalPrice,
    decimal DiscountPrice,
    int Quantity, 
    decimal LineTotal,
    string StockStatus,
    int AvailableQuantity
);

public sealed record CartDto(
    Guid Id, 
    IReadOnlyList<CartItemDto> Items, 
    decimal Subtotal,
    decimal TotalDiscount,
    string? AppliedCouponCode,
    decimal CouponDiscount,
    decimal ShippingCharges,
    decimal EstimatedTax,
    decimal GrandTotal
);
public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
public sealed record UpdateCartItemRequest(int Quantity);
public sealed record CheckoutRequest(
    string ShippingAddress, 
    string PaymentProvider = "stripe", 
    string Currency = "inr",
    string? BillingAddress = null,
    string? OrderNotes = null,
    string? DeliveryInstructions = null
);
public sealed record OrderItemDto(
    Guid ProductId, 
    string ProductName, 
    decimal UnitPrice, 
    int Quantity, 
    decimal LineTotal,
    Guid? ProductVariantId = null,
    string Sku = "",
    decimal Discount = 0,
    decimal Tax = 0,
    decimal Total = 0
);

public sealed record OrderDto(
    Guid Id, 
    string OrderNumber, 
    OrderStatus Status, 
    decimal TotalAmount, 
    string ShippingAddress, 
    IReadOnlyList<OrderItemDto> Items, 
    PaymentDto? Payment,
    string? BillingAddress = null,
    string? PaymentMethod = null,
    string? CourierPartner = null,
    string? TrackingNumber = null,
    decimal ShippingCost = 0,
    decimal Tax = 0,
    decimal Discount = 0,
    string? OrderNotes = null,
    DateTime? EstimatedDeliveryDate = null,
    DateTime? DeliveredDate = null
);

public sealed record UpdateOrderStatusRequest(OrderStatus Status);
public sealed record OrderStatusHistoryDto(Guid Id, OrderStatus Status, string Note, string ChangedBy, DateTime CreatedAtUtc);
public sealed record OrderTrackingDto(Guid Id, string CourierPartner, string TrackingNumber, string Status, string Location, DateTime CreatedAtUtc);

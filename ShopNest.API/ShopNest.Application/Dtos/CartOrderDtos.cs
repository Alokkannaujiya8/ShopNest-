using ShopNest.Domain.Enums;

namespace ShopNest.Application.Dtos;

public sealed record CartItemDto(Guid Id, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public sealed record CartDto(Guid Id, IReadOnlyList<CartItemDto> Items, decimal Total);
public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
public sealed record UpdateCartItemRequest(int Quantity);
public sealed record CheckoutRequest(string ShippingAddress, string PaymentProvider = "stripe", string Currency = "inr");
public sealed record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public sealed record OrderDto(Guid Id, string OrderNumber, OrderStatus Status, decimal TotalAmount, string ShippingAddress, IReadOnlyList<OrderItemDto> Items, PaymentDto? Payment);
public sealed record UpdateOrderStatusRequest(OrderStatus Status);

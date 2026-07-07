using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class CartOrderService(
    ShopNestDbContext db,
    IOrderEventPublisher events,
    IHubContext<OrderHub> hub,
    IBackgroundJobClient jobs,
    IInventoryService inventoryService,
    INotificationService notificationService) : ICartOrderService
{
    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0) throw new InvalidOperationException("Quantity must be greater than zero.");
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var product = await db.Products.FindAsync([request.ProductId], cancellationToken) ?? throw new InvalidOperationException("Product not found.");
        if (!product.IsActive || !product.IsPublished || product.IsDeleted) throw new InvalidOperationException("Product is not available.");
        if (product.StockQuantity < request.Quantity) throw new InvalidOperationException("Insufficient stock.");

        var item = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (item is null)
        {
            cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
        }
        else
        {
            if (product.StockQuantity < item.Quantity + request.Quantity) throw new InvalidOperationException("Insufficient stock.");
            item.Quantity += request.Quantity;
        }

        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new InvalidOperationException("Cart item not found.");

        if (request.Quantity <= 0)
        {
            db.CartItems.Remove(item);
        }
        else
        {
            var product = await db.Products.FindAsync([item.ProductId], cancellationToken) ?? throw new InvalidOperationException("Product not found.");
            if (product.StockQuantity < request.Quantity) throw new InvalidOperationException("Insufficient stock.");
            item.Quantity = request.Quantity;
        }

        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> IncreaseCartItemQuantityAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new InvalidOperationException("Cart item not found.");
        var product = await db.Products.FindAsync([item.ProductId], cancellationToken) ?? throw new InvalidOperationException("Product not found.");
        if (product.StockQuantity < item.Quantity + 1) throw new InvalidOperationException("Insufficient stock.");

        item.Quantity += 1;
        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> DecreaseCartItemQuantityAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new InvalidOperationException("Cart item not found.");

        if (item.Quantity <= 1)
        {
            db.CartItems.Remove(item);
        }
        else
        {
            item.Quantity -= 1;
        }

        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new InvalidOperationException("Cart item not found.");
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> ClearCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        db.CartItems.RemoveRange(cart.Items);
        cart.AppliedCouponCode = null;
        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> ApplyCouponAsync(Guid userId, string couponCode, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var coupon = await db.Coupons.FirstOrDefaultAsync(x => x.Code == couponCode && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Invalid or inactive coupon code.");
        if (coupon.ExpiresAtUtc < DateTime.UtcNow) throw new InvalidOperationException("Coupon has expired.");
        if (coupon.UsageLimit > 0 && coupon.UsageCount >= coupon.UsageLimit) throw new InvalidOperationException("Coupon usage limit reached.");

        var subtotal = cart.Items.Sum(x => x.Product.Price * x.Quantity);
        if (subtotal < coupon.MinOrderAmount) throw new InvalidOperationException($"Minimum order of ${coupon.MinOrderAmount} required.");

        cart.AppliedCouponCode = coupon.Code;
        await db.SaveChangesAsync(cancellationToken);

        await notificationService.SendManualNotificationAsync(new SendManualNotificationRequest(
            userId,
            "Coupon Applied",
            $"Coupon '{coupon.Code}' has been successfully applied to your shopping cart.",
            "Success",
            "InApp",
            "Low"
        ), cancellationToken);

        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> RemoveCouponAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        cart.AppliedCouponCode = null;
        await db.SaveChangesAsync(cancellationToken);
        return await MapCartToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> MoveWishlistItemToCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var wishItem = await db.WishlistItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);
        if (wishItem is not null)
        {
            db.WishlistItems.Remove(wishItem);
        }
        return await AddToCartAsync(userId, new AddCartItemRequest(productId, 1), cancellationToken);
    }

    public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        if (cart.Items.Count == 0) throw new InvalidOperationException("Cart is empty.");

        foreach (var item in cart.Items)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"{item.Product.Name} has insufficient stock.");
            }
        }

        // Apply coupon discount if any
        decimal subtotal = cart.Items.Sum(x => x.Product.Price * x.Quantity);
        decimal couponDiscount = 0;
        Coupon? coupon = null;
        if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
        {
            coupon = await db.Coupons.FirstOrDefaultAsync(x => x.Code == cart.AppliedCouponCode && x.IsActive, cancellationToken);
            if (coupon != null && coupon.ExpiresAtUtc > DateTime.UtcNow && subtotal >= coupon.MinOrderAmount)
            {
                if (string.Equals(coupon.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                {
                    couponDiscount = subtotal * (coupon.DiscountValue / 100);
                }
                else if (string.Equals(coupon.DiscountType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
                {
                    couponDiscount = coupon.DiscountValue;
                }

                if (coupon.MaxDiscountAmount.HasValue && couponDiscount > coupon.MaxDiscountAmount.Value)
                {
                    couponDiscount = coupon.MaxDiscountAmount.Value;
                }

                coupon.UsageCount += 1;
            }
        }

        var shippingCharges = (subtotal > 0 && subtotal < 100) ? 10m : 0m;
        var estimatedTax = (subtotal - couponDiscount) * 0.10m;
        if (estimatedTax < 0) estimatedTax = 0;
        var grandTotal = subtotal - couponDiscount + shippingCharges + estimatedTax;

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"SN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            ShippingAddress = request.ShippingAddress.Trim(),
            BillingAddress = request.BillingAddress ?? request.ShippingAddress.Trim(),
            PaymentMethod = request.PaymentProvider,
            ShippingCost = shippingCharges,
            Tax = estimatedTax,
            Discount = couponDiscount,
            OrderNotes = request.OrderNotes,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7),
            TotalAmount = grandTotal,
            Status = OrderStatus.Pending,
            Items = cart.Items.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitPrice = x.Product.Price,
                Quantity = x.Quantity,
                Sku = x.Product.Sku ?? string.Empty,
                Discount = 0,
                Tax = x.Product.Price * x.Quantity * 0.10m,
                Total = x.Product.Price * x.Quantity
            }).ToList()
        };

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Pending,
            Note = "Order placed successfully.",
            ChangedBy = "System"
        });

        foreach (var item in cart.Items)
        {
            await inventoryService.StockOutAsync(new StockOutRequest(
                item.ProductId,
                null,
                null,
                item.Quantity,
                "System / Checkout",
                $"Order placed: {order.OrderNumber}",
                order.OrderNumber
            ), cancellationToken);
        }

        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart.Items);
        cart.AppliedCouponCode = null;
        await db.SaveChangesAsync(cancellationToken);

        await events.PublishOrderCreatedAsync(order, cancellationToken);
        jobs.Enqueue<IOrderNotificationService>(x => x.SendOrderConfirmationAsync(order.Id));

        await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
        {
            type = "NewOrderCreated",
            message = $"New Order {order.OrderNumber} created for {order.TotalAmount:C}.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        var created = await OrderQuery().FirstAsync(x => x.Id == order.Id, cancellationToken);
        return created.ToDto();
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(Guid? userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = OrderQuery().AsNoTracking();
        if (userId is not null) query = query.Where(x => x.UserId == userId);

        var total = await query.CountAsync(cancellationToken);
        var orders = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<OrderDto>(orders.Select(x => x.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<OrderDto?> GetOrderAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = OrderQuery().AsNoTracking().Where(x => x.Id == orderId);
        if (userId is not null) query = query.Where(x => x.UserId == userId);
        return (await query.FirstOrDefaultAsync(cancellationToken))?.ToDto();
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return null;

        order.Status = request.Status;
        order.UpdatedAtUtc = DateTime.UtcNow;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = request.Status,
            Note = $"Status updated to {request.Status} by Admin",
            ChangedBy = "Admin"
        });

        await db.SaveChangesAsync(cancellationToken);
        await events.PublishOrderStatusChangedAsync(order, cancellationToken);

        var dto = order.ToDto();
        await hub.Clients.Group(order.UserId.ToString()).SendAsync("orderStatusChanged", dto, cancellationToken);

        if (request.Status == OrderStatus.Shipped)
        {
            await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
            {
                type = "OrderShipped",
                message = $"Your order {order.OrderNumber} has been shipped!",
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
        else if (request.Status == OrderStatus.Delivered)
        {
            order.DeliveredDate = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
            {
                type = "OrderDelivered",
                message = $"Your order {order.OrderNumber} has been delivered!",
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
        else if (request.Status == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                await inventoryService.StockInAsync(new StockInRequest(
                    item.ProductId,
                    null,
                    null,
                    item.Quantity,
                    0,
                    "System / Cancellation",
                    $"Order cancelled: {order.OrderNumber}",
                    order.OrderNumber
                ), cancellationToken);
            }

            await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
            {
                type = "OrderCancelled",
                message = $"Your order {order.OrderNumber} has been cancelled.",
                timestamp = DateTime.UtcNow
            }, cancellationToken);

            await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
            {
                type = "OrderCancelled",
                message = $"Order {order.OrderNumber} was cancelled.",
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }

        return dto;
    }

    public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, Guid? userId, CancellationToken cancellationToken)
    {
        var query = OrderQuery().AsNoTracking().Where(x => x.OrderNumber == orderNumber);
        if (userId is not null) query = query.Where(x => x.UserId == userId);
        return (await query.FirstOrDefaultAsync(cancellationToken))?.ToDto();
    }

    public async Task<OrderDto?> CancelOrderAsync(Guid orderId, Guid? userId, string reason, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return null;
        if (userId is not null && order.UserId != userId) return null;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            await inventoryService.StockInAsync(new StockInRequest(
                item.ProductId,
                null,
                null,
                item.Quantity,
                0,
                "System / Cancellation",
                $"Order cancelled: {order.OrderNumber}",
                order.OrderNumber
            ), cancellationToken);
        }

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            Note = $"Cancelled: {reason}",
            ChangedBy = userId is null ? "Admin" : "Customer"
        });

        await db.SaveChangesAsync(cancellationToken);
        await events.PublishOrderStatusChangedAsync(order, cancellationToken);

        var dto = order.ToDto();
        await hub.Clients.Group(order.UserId.ToString()).SendAsync("orderStatusChanged", dto, cancellationToken);
        await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
        {
            type = "OrderCancelled",
            message = $"Your order {order.OrderNumber} has been cancelled.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
        {
            type = "OrderCancelled",
            message = $"Order {order.OrderNumber} was cancelled.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return dto;
    }

    public async Task<OrderDto?> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return null;

        if (order.Payment is not null)
        {
            if (Enum.TryParse<PaymentStatus>(paymentStatus, true, out var parsedStatus))
            {
                order.Payment.Status = parsedStatus;
                if (parsedStatus == PaymentStatus.Succeeded)
                {
                    order.Status = OrderStatus.PaymentCompleted;
                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        Status = OrderStatus.PaymentCompleted,
                        Note = "Payment succeeded.",
                        ChangedBy = "PaymentGateway"
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return order.ToDto();
    }

    public async Task<OrderDto?> RequestReturnAsync(Guid orderId, Guid userId, string reason, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null || order.UserId != userId) return null;

        order.Status = OrderStatus.ReturnRequested;
        order.ReturnRequests.Add(new ReturnRequest
        {
            OrderId = order.Id,
            Reason = reason,
            Status = "Pending"
        });

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.ReturnRequested,
            Note = $"Return requested: {reason}",
            ChangedBy = "Customer"
        });

        await db.SaveChangesAsync(cancellationToken);
        
        await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
        {
            type = "ReturnRequested",
            message = $"Return requested for Order {order.OrderNumber}.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return order.ToDto();
    }

    public async Task<OrderDto?> RequestRefundAsync(Guid orderId, Guid userId, string reason, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null || order.UserId != userId) return null;

        order.Status = OrderStatus.RefundRequested;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.RefundRequested,
            Note = $"Refund requested: {reason}",
            ChangedBy = "Customer"
        });

        await db.SaveChangesAsync(cancellationToken);

        await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
        {
            type = "RefundRequested",
            message = $"Refund requested for Order {order.OrderNumber}.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return order.ToDto();
    }

    public async Task<OrderDto?> AssignCourierAsync(Guid orderId, string courierPartner, string trackingNumber, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return null;

        order.CourierPartner = courierPartner;
        order.TrackingNumber = trackingNumber;
        order.Status = OrderStatus.ReadyToShip;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.ReadyToShip,
            Note = $"Courier assigned: {courierPartner}. Tracking Number: {trackingNumber}",
            ChangedBy = "Admin"
        });

        order.TrackingUpdates.Add(new OrderTracking
        {
            OrderId = order.Id,
            CourierPartner = courierPartner,
            TrackingNumber = trackingNumber,
            Status = "Label Created",
            Location = "Warehouse"
        });

        await db.SaveChangesAsync(cancellationToken);

        await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
        {
            type = "OrderReadyToShip",
            message = $"Your order {order.OrderNumber} is ready to ship with {courierPartner}.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return order.ToDto();
    }

    public async Task<List<OrderStatusHistoryDto>> GetOrderTimelineAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = db.OrderStatusHistories.AsNoTracking().Where(x => x.OrderId == orderId);
        if (userId is not null)
        {
            var isOwner = await db.Orders.AnyAsync(x => x.Id == orderId && x.UserId == userId, cancellationToken);
            if (!isOwner) return new List<OrderStatusHistoryDto>();
        }
        var histories = await query.OrderBy(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return histories.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<OrderTrackingDto>> GetOrderTrackingAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = db.OrderTrackings.AsNoTracking().Where(x => x.OrderId == orderId);
        if (userId is not null)
        {
            var isOwner = await db.Orders.AnyAsync(x => x.Id == orderId && x.UserId == userId, cancellationToken);
            if (!isOwner) return new List<OrderTrackingDto>();
        }
        var trackings = await query.OrderBy(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return trackings.Select(x => x.ToDto()).ToList();
    }

    public async Task<OrderDto?> AddTrackingUpdateAsync(Guid orderId, string status, string location, CancellationToken cancellationToken)
    {
        var order = await OrderQuery().FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return null;

        order.TrackingUpdates.Add(new OrderTracking
        {
            OrderId = order.Id,
            CourierPartner = order.CourierPartner ?? "Unknown",
            TrackingNumber = order.TrackingNumber ?? "None",
            Status = status,
            Location = location
        });

        if (status.Equals("Shipped", StringComparison.OrdinalIgnoreCase))
        {
            order.Status = OrderStatus.Shipped;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Shipped,
                Note = $"Order shipped. Location: {location}",
                ChangedBy = "Courier"
            });
        }
        else if (status.Equals("Out For Delivery", StringComparison.OrdinalIgnoreCase))
        {
            order.Status = OrderStatus.OutForDelivery;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.OutForDelivery,
                Note = "Order out for delivery.",
                ChangedBy = "Courier"
            });
        }
        else if (status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
        {
            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Delivered,
                Note = "Order delivered successfully.",
                ChangedBy = "Courier"
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
        {
            type = "OrderTrackingUpdated",
            message = $"Order {order.OrderNumber} tracking updated: {status} at {location}.",
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return order.ToDto();
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(p => p.Category)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(p => p.Brand)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (cart is not null) return cart;

        cart = new Cart { UserId = userId };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(cancellationToken);

        return await db.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(p => p.Category)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(p => p.Brand)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(p => p.Images)
            .FirstAsync(x => x.Id == cart.Id, cancellationToken);
    }

    private async Task<CartDto> MapCartToDtoAsync(Cart cart, CancellationToken cancellationToken)
    {
        Coupon? coupon = null;
        if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
        {
            coupon = await db.Coupons
                .FirstOrDefaultAsync(x => x.Code == cart.AppliedCouponCode && x.IsActive, cancellationToken);
        }
        return cart.ToDto(coupon);
    }

    private IQueryable<Order> OrderQuery() => db.Orders
        .Include(x => x.Items)
        .Include(x => x.Payment)
        .Include(x => x.StatusHistory)
        .Include(x => x.TrackingUpdates)
        .Include(x => x.ReturnRequests);
}

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
    IBackgroundJobClient jobs) : ICartOrderService
{
    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return cart.ToDto();
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0) throw new InvalidOperationException("Quantity must be greater than zero.");
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var product = await db.Products.FindAsync([request.ProductId], cancellationToken) ?? throw new InvalidOperationException("Product not found.");
        if (product.StockQuantity < request.Quantity) throw new InvalidOperationException("Insufficient stock.");

        var item = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (item is null)
        {
            cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
        }
        else
        {
            item.Quantity += request.Quantity;
        }

        await db.SaveChangesAsync(cancellationToken);
        return (await GetOrCreateCartAsync(userId, cancellationToken)).ToDto();
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new InvalidOperationException("Cart item not found.");

        if (request.Quantity <= 0) db.CartItems.Remove(item);
        else item.Quantity = request.Quantity;

        await db.SaveChangesAsync(cancellationToken);
        return (await GetOrCreateCartAsync(userId, cancellationToken)).ToDto();
    }

    public async Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new InvalidOperationException("Cart item not found.");
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return (await GetOrCreateCartAsync(userId, cancellationToken)).ToDto();
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

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"SN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            ShippingAddress = request.ShippingAddress.Trim(),
            TotalAmount = cart.Items.Sum(x => x.Product.Price * x.Quantity),
            Items = cart.Items.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitPrice = x.Product.Price,
                Quantity = x.Quantity
            }).ToList()
        };

        foreach (var item in cart.Items)
        {
            item.Product.StockQuantity -= item.Quantity;
            if (item.Product.StockQuantity <= 5)
            {
                await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
                {
                    type = "ProductStockLow",
                    message = $"{item.Product.Name} is running low on stock ({item.Product.StockQuantity} remaining).",
                    timestamp = DateTime.UtcNow
                }, cancellationToken);
            }
        }

        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart.Items);
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
            await hub.Clients.Group(order.UserId.ToString()).SendAsync("notificationReceived", new
            {
                type = "OrderDelivered",
                message = $"Your order {order.OrderNumber} has been delivered!",
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
        else if (request.Status == OrderStatus.Cancelled)
        {
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

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await db.Carts.Include(x => x.Items).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (cart is not null) return cart;

        cart = new Cart { UserId = userId };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(cancellationToken);
        return await db.Carts.Include(x => x.Items).ThenInclude(x => x.Product).FirstAsync(x => x.Id == cart.Id, cancellationToken);
    }

    private IQueryable<Order> OrderQuery() => db.Orders.Include(x => x.Items).Include(x => x.Payment);
}

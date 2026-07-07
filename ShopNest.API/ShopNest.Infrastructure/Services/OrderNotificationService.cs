using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopNest.Application.Interfaces;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class OrderNotificationService(
    ShopNestDbContext db, 
    ILogger<OrderNotificationService> logger,
    INotificationService notificationService
) : IOrderNotificationService
{
    public async Task SendOrderConfirmationAsync(Guid orderId)
    {
        var order = await db.Orders.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null) return;

        logger.LogInformation("Order confirmation email queued for {Email} and order {OrderNumber}", order.User.Email, order.OrderNumber);
        
        await notificationService.SendTemplatedNotificationAsync(
            order.UserId,
            "OrderConfirmation",
            new Dictionary<string, string>
            {
                { "OrderNumber", order.OrderNumber },
                { "TotalAmount", order.TotalAmount.ToString("F2") }
            },
            "Order",
            order.Id.ToString(),
            default);
    }
}

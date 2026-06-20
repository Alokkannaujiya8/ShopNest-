using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopNest.Application.Interfaces;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class OrderNotificationService(ShopNestDbContext db, ILogger<OrderNotificationService> logger) : IOrderNotificationService
{
    public async Task SendOrderConfirmationAsync(Guid orderId)
    {
        var order = await db.Orders.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null) return;
        logger.LogInformation("Order confirmation email queued for {Email} and order {OrderNumber}", order.User.Email, order.OrderNumber);
    }
}

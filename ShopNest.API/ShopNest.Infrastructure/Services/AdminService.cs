using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class AdminService(ShopNestDbContext db) : IAdminService
{
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var customers = await db.Users.CountAsync(x => x.Role == UserRole.Customer, cancellationToken);
        var products = await db.Products.CountAsync(cancellationToken);
        var lowStock = await db.Products.CountAsync(x => x.StockQuantity <= 5, cancellationToken);
        var orders = await db.Orders.CountAsync(cancellationToken);
        var revenue = await db.Payments.Where(x => x.Status == PaymentStatus.Succeeded).SumAsync(x => x.Amount, cancellationToken);
        var pending = await db.Orders.CountAsync(x => x.Status == OrderStatus.Pending, cancellationToken);
        return new DashboardStatsDto(customers, products, lowStock, orders, revenue, pending);
    }

    public async Task<bool> UpdateInventoryAsync(Guid productId, InventoryUpdateRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([productId], cancellationToken);
        if (product is null) return false;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

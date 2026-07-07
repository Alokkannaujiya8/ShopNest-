using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class ReportingService(ShopNestDbContext db) : IReportingService
{
    private static readonly OrderStatus[] PaidStatuses =
    [
        OrderStatus.PaymentCompleted,
        OrderStatus.Processing,
        OrderStatus.Packed,
        OrderStatus.ReadyToShip,
        OrderStatus.Shipped,
        OrderStatus.OutForDelivery,
        OrderStatus.Delivered
    ];

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var totalUsers = await db.Users.CountAsync(cancellationToken);
        var totalCustomers = await db.Users.CountAsync(u => u.Role == UserRole.Customer, cancellationToken);
        var totalAdmins = await db.Users.CountAsync(u => u.Role == UserRole.Admin, cancellationToken);

        var totalProducts = await db.Products.CountAsync(cancellationToken);
        var activeProducts = await db.Products.CountAsync(p => p.IsActive && p.IsPublished, cancellationToken);
        var outOfStockProducts = await db.Products.CountAsync(p => p.StockQuantity <= 0, cancellationToken);
        var lowStockProducts = await db.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= p.MinimumStock, cancellationToken);

        var totalCategories = await db.Categories.CountAsync(cancellationToken);
        var totalBrands = await db.Brands.CountAsync(cancellationToken);

        var totalOrders = await db.Orders.CountAsync(cancellationToken);
        var pendingOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var processingOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Processing, cancellationToken);
        var deliveredOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
        var cancelledOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken);
        var returnedOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Returned, cancellationToken);

        var paidOrdersQuery = db.Orders.Where(o => PaidStatuses.Contains(o.Status));
        var totalRevenue = await paidOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken);

        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthlyRevenue = await paidOrdersQuery
            .Where(o => o.CreatedAtUtc >= firstDayOfMonth)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var today = DateTime.UtcNow.Date;
        var todayRevenue = await paidOrdersQuery
            .Where(o => o.CreatedAtUtc >= today)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var paidOrdersCount = await paidOrdersQuery.CountAsync(cancellationToken);
        var averageOrderValue = paidOrdersCount > 0 ? totalRevenue / paidOrdersCount : 0;

        return new DashboardSummaryDto(
            totalUsers,
            totalCustomers,
            totalAdmins,
            totalProducts,
            activeProducts,
            outOfStockProducts,
            lowStockProducts,
            totalCategories,
            totalBrands,
            totalOrders,
            pendingOrders,
            processingOrders,
            deliveredOrders,
            cancelledOrders,
            returnedOrders,
            totalRevenue,
            monthlyRevenue,
            todayRevenue,
            averageOrderValue
        );
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var ordersQuery = db.Orders.Where(o => PaidStatuses.Contains(o.Status) && o.CreatedAtUtc >= start && o.CreatedAtUtc <= end);

        var dailyData = await ordersQuery
            .GroupBy(o => o.CreatedAtUtc.Date)
            .Select(g => new SalesIntervalDto(g.Key.ToString("yyyy-MM-dd"), g.Sum(o => o.TotalAmount), g.Count()))
            .ToListAsync(cancellationToken);

        var monthlyData = await ordersQuery
            .GroupBy(o => new { o.CreatedAtUtc.Year, o.CreatedAtUtc.Month })
            .Select(g => new SalesIntervalDto($"{g.Key.Year}-{g.Key.Month:D2}", g.Sum(o => o.TotalAmount), g.Count()))
            .ToListAsync(cancellationToken);

        var yearlyData = await ordersQuery
            .GroupBy(o => o.CreatedAtUtc.Year)
            .Select(g => new SalesIntervalDto(g.Key.ToString(), g.Sum(o => o.TotalAmount), g.Count()))
            .ToListAsync(cancellationToken);

        // Group weekly in memory to avoid date math SQL translation issues
        var weeklyData = dailyData
            .GroupBy(d => {
                var parsedDate = DateTime.Parse(d.Interval);
                var diff = (7 + (parsedDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                return parsedDate.AddDays(-1 * diff).ToString("yyyy-MM-dd");
            })
            .Select(g => new SalesIntervalDto(g.Key, g.Sum(x => x.TotalSales), g.Sum(x => x.OrderCount)))
            .ToList();

        var itemsQuery = db.OrderItems.Where(oi => PaidStatuses.Contains(oi.Order.Status) && oi.Order.CreatedAtUtc >= start && oi.Order.CreatedAtUtc <= end);

        if (categoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(oi => oi.Product.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            itemsQuery = itemsQuery.Where(oi => oi.Product.BrandId == brandId.Value);
        }

        var salesByCategory = await itemsQuery
            .GroupBy(oi => oi.Product.Category.Name)
            .Select(g => new CategorySalesDto(g.Key, g.Sum(oi => oi.Total), g.Sum(oi => oi.Quantity)))
            .ToListAsync(cancellationToken);

        var salesByBrand = await itemsQuery
            .GroupBy(oi => oi.Product.Brand != null ? oi.Product.Brand.Name : "No Brand")
            .Select(g => new BrandSalesDto(g.Key, g.Sum(oi => oi.Total), g.Sum(oi => oi.Quantity)))
            .ToListAsync(cancellationToken);

        var salesByProduct = await itemsQuery
            .GroupBy(oi => new { oi.ProductName, oi.Sku })
            .Select(g => new ProductSalesDto(g.Key.ProductName, g.Key.Sku, g.Sum(oi => oi.Total), g.Sum(oi => oi.Quantity)))
            .ToListAsync(cancellationToken);

        var topSellingProducts = salesByProduct
            .OrderByDescending(p => p.QuantitySold)
            .Take(10)
            .ToList();

        var topSellingCategories = salesByCategory
            .OrderByDescending(c => c.Revenue)
            .Take(10)
            .ToList();

        var revenueTrends = dailyData
            .Select(d => new RevenueTrendDto(DateTime.Parse(d.Interval), d.TotalSales, d.OrderCount))
            .OrderBy(r => r.Date)
            .ToList();

        return new SalesReportDto(
            dailyData,
            weeklyData,
            monthlyData,
            yearlyData,
            salesByCategory,
            salesByBrand,
            salesByProduct,
            topSellingProducts,
            topSellingCategories,
            revenueTrends
        );
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var ordersQuery = db.Orders.Where(o => PaidStatuses.Contains(o.Status) && o.CreatedAtUtc >= start && o.CreatedAtUtc <= end);

        var stats = await ordersQuery
            .Select(o => new { o.TotalAmount, o.Discount, o.Tax, o.ShippingCost })
            .ToListAsync(cancellationToken);

        var netRevenue = stats.Sum(s => s.TotalAmount);
        var discountGiven = stats.Sum(s => s.Discount);
        var grossRevenue = netRevenue + discountGiven;
        var totalTax = stats.Sum(s => s.Tax);
        var totalShipping = stats.Sum(s => s.ShippingCost);

        var dailyRevenue = await ordersQuery
            .GroupBy(o => o.CreatedAtUtc.Date)
            .Select(g => new RevenueTrendDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
            .OrderBy(r => r.Date)
            .ToListAsync(cancellationToken);

        return new RevenueReportDto(
            grossRevenue,
            discountGiven,
            netRevenue,
            totalTax,
            totalShipping,
            dailyRevenue
        );
    }

    public async Task<OrdersReportDto> GetOrdersReportAsync(DateTime? startDate, DateTime? endDate, string? status, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = db.Orders.Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc <= end);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        var list = await query
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var totalOrders = list.Count;
        var pending = list.Count(o => o.Status == OrderStatus.Pending);
        var delivered = list.Count(o => o.Status == OrderStatus.Delivered);
        var cancelled = list.Count(o => o.Status == OrderStatus.Cancelled);
        var returned = list.Count(o => o.Status == OrderStatus.Returned);

        var totalRefunded = await db.Refunds
            .Where(r => r.Status == "Completed" && r.CreatedAtUtc >= start && r.CreatedAtUtc <= end)
            .SumAsync(r => r.Amount, cancellationToken);

        var dtos = list.Select(o => new OrderReportItemDto(
            o.Id,
            o.OrderNumber,
            o.User?.FullName ?? "Unknown",
            o.Status.ToString(),
            o.TotalAmount,
            o.CreatedAtUtc,
            o.PaymentMethod
        )).ToList();

        return new OrdersReportDto(
            totalOrders,
            pending,
            delivered,
            cancelled,
            returned,
            totalRefunded,
            dtos
        );
    }

    public async Task<CustomerReportDto> GetCustomerReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var totalCustomers = await db.Users.CountAsync(u => u.Role == UserRole.Customer, cancellationToken);
        var newCustomersCount = await db.Users.CountAsync(u => u.Role == UserRole.Customer && u.CreatedAtUtc >= start && u.CreatedAtUtc <= end, cancellationToken);

        var activeCustomersCount = await db.Orders
            .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc <= end)
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var topCustomers = await db.Orders
            .Where(o => PaidStatuses.Contains(o.Status) && o.CreatedAtUtc >= start && o.CreatedAtUtc <= end)
            .GroupBy(o => new { o.UserId, o.User.FullName, o.User.Email })
            .Select(g => new CustomerReportItemDto(g.Key.UserId, g.Key.FullName, g.Key.Email, g.Count(), g.Sum(x => x.TotalAmount)))
            .OrderByDescending(x => x.TotalSpent)
            .Take(10)
            .ToListAsync(cancellationToken);

        var history = await db.Orders
            .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc <= end)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(50)
            .Select(o => new CustomerPurchaseHistoryDto(
                o.UserId,
                o.User.FullName,
                o.OrderNumber,
                o.TotalAmount,
                o.CreatedAtUtc,
                o.Status.ToString()
            ))
            .ToListAsync(cancellationToken);

        return new CustomerReportDto(
            totalCustomers,
            newCustomersCount,
            activeCustomersCount,
            topCustomers,
            history
        );
    }

    public async Task<ProductReportDto> GetProductReportAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var itemsQuery = db.OrderItems.Where(oi => PaidStatuses.Contains(oi.Order.Status) && oi.Order.CreatedAtUtc >= start && oi.Order.CreatedAtUtc <= end);

        if (categoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(oi => oi.Product.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            itemsQuery = itemsQuery.Where(oi => oi.Product.BrandId == brandId.Value);
        }

        var perfList = await itemsQuery
            .GroupBy(oi => new { oi.ProductId, oi.ProductName, oi.Sku })
            .Select(g => new ProductPerformanceDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.Sku,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.Total)
            ))
            .OrderByDescending(x => x.QuantitySold)
            .ToListAsync(cancellationToken);

        var productsQuery = db.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.BrandId == brandId.Value);
        }

        var lowStock = await productsQuery
            .Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.MinimumStock)
            .Select(p => new ProductStockDto(p.Id, p.Name, p.Sku, p.StockQuantity, p.Price))
            .ToListAsync(cancellationToken);

        var outOfStock = await productsQuery
            .Where(p => p.StockQuantity <= 0)
            .Select(p => new ProductStockDto(p.Id, p.Name, p.Sku, p.StockQuantity, p.Price))
            .ToListAsync(cancellationToken);

        var bestRated = await db.Reviews
            .Where(r => r.Status == ReviewStatus.Approved)
            .GroupBy(r => new { r.ProductId, r.Product.Name })
            .Select(g => new ProductRatingDto(g.Key.ProductId, g.Key.Name, g.Average(r => r.Rating), g.Count()))
            .OrderByDescending(x => x.AverageRating)
            .Take(10)
            .ToListAsync(cancellationToken);

        var worstRated = await db.Reviews
            .Where(r => r.Status == ReviewStatus.Approved)
            .GroupBy(r => new { r.ProductId, r.Product.Name })
            .Select(g => new ProductRatingDto(g.Key.ProductId, g.Key.Name, g.Average(r => r.Rating), g.Count()))
            .OrderBy(x => x.AverageRating)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new ProductReportDto(
            perfList,
            lowStock,
            outOfStock,
            bestRated,
            worstRated
        );
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync(CancellationToken cancellationToken)
    {
        var outOfStock = await db.Inventories.CountAsync(i => i.AvailableStock <= 0, cancellationToken);
        var lowStock = await db.Inventories.CountAsync(i => i.AvailableStock > 0 && i.AvailableStock <= i.MinimumStockLevel, cancellationToken);
        var inStock = await db.Inventories.CountAsync(i => i.AvailableStock > i.MinimumStockLevel, cancellationToken);
        var totalStock = await db.Inventories.SumAsync(i => i.AvailableStock, cancellationToken);

        var recentTxns = await db.InventoryTransactions
            .Include(t => t.Inventory.Product)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(20)
            .Select(t => new InventoryTransactionSummaryDto(
                t.Id,
                t.Inventory.Product.Name,
                t.Inventory.Sku,
                t.TransactionType,
                t.Quantity,
                t.Reason ?? "-",
                t.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new InventoryReportDto(
            outOfStock,
            lowStock,
            inStock,
            totalStock,
            recentTxns
        );
    }

    public async Task<PaymentReportDto> GetPaymentReportAsync(DateTime? startDate, DateTime? endDate, string? method, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = db.Payments.Include(p => p.Order).ThenInclude(o => o.User).Where(p => p.CreatedAtUtc >= start && p.CreatedAtUtc <= end);

        if (!string.IsNullOrWhiteSpace(method))
        {
            query = query.Where(p => p.Provider == method);
        }

        var list = await query.ToListAsync(cancellationToken);

        var success = list.Count(p => p.Status == PaymentStatus.Succeeded);
        var failed = list.Count(p => p.Status == PaymentStatus.Failed);
        var pending = list.Count(p => p.Status == PaymentStatus.Pending);

        var totalRefunded = await db.Refunds
            .Where(r => r.Status == "Completed" && r.CreatedAtUtc >= start && r.CreatedAtUtc <= end)
            .SumAsync(r => r.Amount, cancellationToken);

        var usage = list
            .GroupBy(p => p.Provider)
            .Select(g => new PaymentMethodUsageDto(g.Key, g.Count(), g.Sum(p => p.Amount)))
            .ToList();

        var recent = list
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(50)
            .Select(p => new PaymentReportItemDto(
                p.Id,
                p.Order?.OrderNumber ?? "-",
                p.Order?.User?.FullName ?? "Guest",
                p.Amount,
                p.Provider,
                p.Status.ToString(),
                p.CreatedAtUtc
            ))
            .ToList();

        return new PaymentReportDto(
            success,
            failed,
            pending,
            totalRefunded,
            usage,
            recent
        );
    }

    public async Task<CouponReportDto> GetCouponReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var usages = await db.CouponUsages
            .Include(cu => cu.Coupon)
            .Include(cu => cu.Order)
            .Where(cu => cu.CreatedAtUtc >= start && cu.CreatedAtUtc <= end)
            .ToListAsync(cancellationToken);

        var usageSummary = usages
            .GroupBy(cu => cu.Coupon.Code)
            .Select(g => new CouponUsageSummaryDto(g.Key, g.Count(), g.Sum(x => x.Order?.Discount ?? 0)))
            .ToList();

        var perf = usages
            .GroupBy(cu => new { cu.Coupon.Code, cu.Coupon.MinOrderAmount, cu.Coupon.DiscountValue, cu.Coupon.DiscountType })
            .Select(g => new CouponPerformanceDto(
                g.Key.Code,
                g.Key.MinOrderAmount,
                g.Key.DiscountValue,
                g.Key.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase),
                g.Count(),
                g.Sum(x => x.Order?.Discount ?? 0)
            ))
            .ToList();

        var totalDiscount = usages.Sum(cu => cu.Order?.Discount ?? 0);

        return new CouponReportDto(
            usageSummary,
            perf,
            totalDiscount
        );
    }

    public async Task<ReviewReportDto> GetReviewReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var reviews = await db.Reviews
            .Where(r => r.Status == ReviewStatus.Approved && r.CreatedAtUtc >= start && r.CreatedAtUtc <= end)
            .ToListAsync(cancellationToken);

        var avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0.0;

        var distribution = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var r in reviews)
        {
            if (distribution.ContainsKey(r.Rating))
            {
                distribution[r.Rating]++;
            }
        }

        var mostReviewed = await db.Reviews
            .Where(r => r.Status == ReviewStatus.Approved)
            .GroupBy(r => new { r.ProductId, r.Product.Name })
            .Select(g => new MostReviewedProductDto(g.Key.ProductId, g.Key.Name, g.Count(), g.Average(r => r.Rating)))
            .OrderByDescending(x => x.ReviewCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new ReviewReportDto(
            avgRating,
            distribution,
            mostReviewed
        );
    }

    public async Task<byte[]> ExportReportAsync(string reportType, string format, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        reportType = reportType.ToLowerInvariant();
        format = format.ToLowerInvariant();

        var title = $"{char.ToUpperInvariant(reportType[0]) + reportType.Substring(1)} Report";
        string[] headers;
        var rows = new List<string[]>();

        if (reportType == "sales")
        {
            var data = await GetSalesReportAsync(startDate, endDate, null, null, cancellationToken);
            headers = ["Interval", "Sales Amount", "Order Count"];
            foreach (var item in data.DailySales)
            {
                rows.Add([item.Interval, $"${item.TotalSales:F2}", item.OrderCount.ToString()]);
            }
        }
        else if (reportType == "revenue")
        {
            var data = await GetRevenueReportAsync(startDate, endDate, cancellationToken);
            headers = ["Metric", "Amount"];
            rows.Add(["Gross Revenue", $"${data.GrossRevenue:F2}"]);
            rows.Add(["Coupon Discounts", $"${data.DiscountGiven:F2}"]);
            rows.Add(["Net Revenue", $"${data.NetRevenue:F2}"]);
            rows.Add(["Estimated Taxes", $"${data.TotalTax:F2}"]);
            rows.Add(["Shipping Costs Collected", $"${data.TotalShippingCost:F2}"]);
        }
        else if (reportType == "orders")
        {
            var data = await GetOrdersReportAsync(startDate, endDate, null, cancellationToken);
            headers = ["Order Number", "Customer", "Status", "Amount", "Date"];
            foreach (var item in data.Orders)
            {
                rows.Add([item.OrderNumber, item.CustomerName, item.Status, $"${item.TotalAmount:F2}", item.CreatedAtUtc.ToString("g")]);
            }
        }
        else if (reportType == "customers")
        {
            var data = await GetCustomerReportAsync(startDate, endDate, cancellationToken);
            headers = ["Customer Name", "Email", "Orders Placed", "Total Spent"];
            foreach (var item in data.TopCustomers)
            {
                rows.Add([item.FullName, item.Email, item.OrderCount.ToString(), $"${item.TotalSpent:F2}"]);
            }
        }
        else if (reportType == "products")
        {
            var data = await GetProductReportAsync(startDate, endDate, null, null, cancellationToken);
            headers = ["Product Name", "SKU", "Units Sold", "Revenue Generated"];
            foreach (var item in data.Performance)
            {
                rows.Add([item.ProductName, item.Sku, item.QuantitySold.ToString(), $"${item.RevenueGenerated:F2}"]);
            }
        }
        else if (reportType == "inventory")
        {
            var data = await GetInventoryReportAsync(cancellationToken);
            headers = ["Out of Stock Count", "Low Stock Count", "In Stock Count", "Total Stock Volume"];
            rows.Add([data.OutOfStockCount.ToString(), data.LowStockCount.ToString(), data.InStockCount.ToString(), data.TotalStockQuantity.ToString()]);
        }
        else if (reportType == "payments")
        {
            var data = await GetPaymentReportAsync(startDate, endDate, null, cancellationToken);
            headers = ["Gateway Provider", "Usage Count", "Total VolumeProcessed"];
            foreach (var item in data.MethodUsage)
            {
                rows.Add([item.Method, item.UsageCount.ToString(), $"${item.TotalAmount:F2}"]);
            }
        }
        else if (reportType == "coupons")
        {
            var data = await GetCouponReportAsync(startDate, endDate, cancellationToken);
            headers = ["Coupon Code", "Usage Count", "Total Discounts Claims"];
            foreach (var item in data.Performance)
            {
                rows.Add([item.Code, item.UsageCount.ToString(), $"${item.TotalSavings:F2}"]);
            }
        }
        else
        {
            var data = await GetReviewReportAsync(startDate, endDate, cancellationToken);
            headers = ["Average Customer Rating", "Approved Reviews Sampled"];
            rows.Add([data.AverageRating.ToString("F2"), data.RatingDistribution.Values.Sum().ToString()]);
        }

        if (format == "pdf")
        {
            return PdfReportWriter.Generate(title, headers, rows);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h.Replace("\"", "\"\"")}\"")));
            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",", row.Select(r => $"\"{r.Replace("\"", "\"\"")}\"")));
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}

public static class PdfReportWriter
{
    public static byte[] Generate(string title, string[] headers, List<string[]> rows)
    {
        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, Encoding.ASCII))
        {
            // PDF basic header
            writer.Write("%PDF-1.4\n");
            
            // Object catalog mappings
            writer.Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
            writer.Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
            writer.Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");
            writer.Write("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

            var contentBuilder = new StringBuilder();
            contentBuilder.Append("BT\n");
            contentBuilder.Append("/F1 18 Tf\n");
            contentBuilder.Append("50 800 Td\n");
            contentBuilder.Append($"({title}) Tj\n");
            contentBuilder.Append("ET\n");

            contentBuilder.Append("BT\n");
            contentBuilder.Append("/F1 10 Tf\n");
            contentBuilder.Append("50 760 Td\n");
            
            var headerStr = string.Join("  |  ", headers);
            contentBuilder.Append($"({headerStr}) Tj\n");
            contentBuilder.Append("0 -15 Td\n");
            contentBuilder.Append($"(-----------------------------------------------------------------------------------------) Tj\n");
            contentBuilder.Append("0 -15 Td\n");

            var yPos = 730;
            foreach (var row in rows)
            {
                var rowStr = string.Join("  |  ", row);
                // Sanitize brackets to prevent PDF token breakages
                rowStr = rowStr.Replace("(", "\\(").Replace(")", "\\)");
                contentBuilder.Append($"({rowStr}) Tj\n");
                contentBuilder.Append("0 -15 Td\n");
                yPos -= 15;
                if (yPos < 60) break;
            }
            contentBuilder.Append("ET\n");

            var contentBytes = Encoding.UTF8.GetBytes(contentBuilder.ToString());
            
            writer.Write($"5 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
            writer.Flush();
            ms.Write(contentBytes, 0, contentBytes.Length);
            writer.Write("\nendstream\nendobj\n");

            writer.Write("xref\n0 6\n0000000000 65535 f \n");
            writer.Flush();

            writer.Write("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n10\n%%EOF\n");
            writer.Flush();
        }

        return ms.ToArray();
    }
}

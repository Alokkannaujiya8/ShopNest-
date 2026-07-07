using System;
using System.Collections.Generic;

namespace ShopNest.Application.Dtos;

public sealed record DashboardSummaryDto(
    int TotalUsers,
    int TotalCustomers,
    int TotalAdmins,
    int TotalProducts,
    int ActiveProducts,
    int OutOfStockProducts,
    int LowStockProducts,
    int TotalCategories,
    int TotalBrands,
    int TotalOrders,
    int PendingOrders,
    int ProcessingOrders,
    int DeliveredOrders,
    int CancelledOrders,
    int ReturnedOrders,
    decimal TotalRevenue,
    decimal MonthlyRevenue,
    decimal TodayRevenue,
    decimal AverageOrderValue
);

public sealed record SalesIntervalDto(
    string Interval,
    decimal TotalSales,
    int OrderCount
);

public sealed record CategorySalesDto(
    string CategoryName,
    decimal Revenue,
    int ItemsSold
);

public sealed record BrandSalesDto(
    string BrandName,
    decimal Revenue,
    int ItemsSold
);

public sealed record ProductSalesDto(
    string ProductName,
    string Sku,
    decimal Revenue,
    int QuantitySold
);

public sealed record RevenueTrendDto(
    DateTime Date,
    decimal Revenue,
    int OrderCount
);

public sealed record SalesReportDto(
    List<SalesIntervalDto> DailySales,
    List<SalesIntervalDto> WeeklySales,
    List<SalesIntervalDto> MonthlySales,
    List<SalesIntervalDto> YearlySales,
    List<CategorySalesDto> SalesByCategory,
    List<BrandSalesDto> SalesByBrand,
    List<ProductSalesDto> SalesByProduct,
    List<ProductSalesDto> TopSellingProducts,
    List<CategorySalesDto> TopSellingCategories,
    List<RevenueTrendDto> RevenueTrends
);

public sealed record RevenueReportDto(
    decimal GrossRevenue,
    decimal DiscountGiven,
    decimal NetRevenue,
    decimal TotalTax,
    decimal TotalShippingCost,
    List<RevenueTrendDto> RevenueTrends
);

public sealed record OrderReportItemDto(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    string? PaymentMethod
);

public sealed record OrdersReportDto(
    int TotalOrders,
    int PendingCount,
    int DeliveredCount,
    int CancelledCount,
    int ReturnedCount,
    decimal TotalRefunded,
    List<OrderReportItemDto> Orders
);

public sealed record CustomerReportItemDto(
    Guid UserId,
    string FullName,
    string Email,
    int OrderCount,
    decimal TotalSpent
);

public sealed record CustomerPurchaseHistoryDto(
    Guid UserId,
    string FullName,
    string OrderNumber,
    decimal Amount,
    DateTime OrderDate,
    string Status
);

public sealed record CustomerReportDto(
    int TotalCustomers,
    int NewCustomersCount,
    int ActiveCustomersCount,
    List<CustomerReportItemDto> TopCustomers,
    List<CustomerPurchaseHistoryDto> PurchaseHistories
);

public sealed record ProductPerformanceDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    int QuantitySold,
    decimal RevenueGenerated
);

public sealed record ProductStockDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    int StockQuantity,
    decimal Price
);

public sealed record ProductRatingDto(
    Guid ProductId,
    string ProductName,
    double AverageRating,
    int ReviewCount
);

public sealed record ProductReportDto(
    List<ProductPerformanceDto> Performance,
    List<ProductStockDto> LowStockProducts,
    List<ProductStockDto> OutOfStockProducts,
    List<ProductRatingDto> BestRated,
    List<ProductRatingDto> WorstRated
);

public sealed record InventoryTransactionSummaryDto(
    Guid TransactionId,
    string ProductName,
    string Sku,
    string TransactionType,
    int Quantity,
    string Reason,
    DateTime CreatedAtUtc
);

public sealed record InventoryReportDto(
    int OutOfStockCount,
    int LowStockCount,
    int InStockCount,
    int TotalStockQuantity,
    List<InventoryTransactionSummaryDto> RecentTransactions
);

public sealed record PaymentMethodUsageDto(
    string Method,
    int UsageCount,
    decimal TotalAmount
);

public sealed record PaymentReportItemDto(
    Guid PaymentId,
    string OrderNumber,
    string CustomerName,
    decimal Amount,
    string Provider,
    string Status,
    DateTime CreatedAtUtc
);

public sealed record PaymentReportDto(
    int SuccessCount,
    int FailedCount,
    int PendingCount,
    decimal TotalRefunded,
    List<PaymentMethodUsageDto> MethodUsage,
    List<PaymentReportItemDto> RecentPayments
);

public sealed record CouponUsageSummaryDto(
    string Code,
    int UsageCount,
    decimal TotalDiscountGiven
);

public sealed record CouponPerformanceDto(
    string Code,
    decimal MinOrderAmount,
    decimal DiscountValue,
    bool IsPercent,
    int UsageCount,
    decimal TotalSavings
);

public sealed record CouponReportDto(
    List<CouponUsageSummaryDto> Usage,
    List<CouponPerformanceDto> Performance,
    decimal TotalDiscountGiven
);

public sealed record MostReviewedProductDto(
    Guid ProductId,
    string ProductName,
    int ReviewCount,
    double AverageRating
);

public sealed record ReviewReportDto(
    double AverageRating,
    Dictionary<int, int> RatingDistribution,
    List<MostReviewedProductDto> MostReviewed
);

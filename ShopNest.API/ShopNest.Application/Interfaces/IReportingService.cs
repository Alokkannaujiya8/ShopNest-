using System;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IReportingService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
    Task<SalesReportDto> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken);
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<OrdersReportDto> GetOrdersReportAsync(DateTime? startDate, DateTime? endDate, string? status, CancellationToken cancellationToken);
    Task<CustomerReportDto> GetCustomerReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<ProductReportDto> GetProductReportAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken);
    Task<InventoryReportDto> GetInventoryReportAsync(CancellationToken cancellationToken);
    Task<PaymentReportDto> GetPaymentReportAsync(DateTime? startDate, DateTime? endDate, string? method, CancellationToken cancellationToken);
    Task<CouponReportDto> GetCouponReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<ReviewReportDto> GetReviewReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    
    Task<byte[]> ExportReportAsync(string reportType, string format, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
}

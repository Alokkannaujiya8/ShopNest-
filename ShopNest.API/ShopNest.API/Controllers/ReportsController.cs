using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Reports;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.API.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IMediator mediator, ShopNestDbContext db) : ControllerBase
{
    private async Task LogAuditAsync(string action, string details, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.UserId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = "Reports",
                EntityId = "System",
                IPAddress = ip,
                Details = details
            });
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Fail-safe to avoid breaking report generation on audit failures
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        await LogAuditAsync("DashboardAccess", "Admin accessed reports dashboard summary.", cancellationToken);
        var result = await mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated sales analytics report.", cancellationToken);
        var result = await mediator.Send(new GetSalesReportQuery(startDate, endDate, categoryId, brandId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDto>> GetRevenueReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated revenue report.", cancellationToken);
        var result = await mediator.Send(new GetRevenueReportQuery(startDate, endDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<OrdersReportDto>> GetOrdersReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated orders report.", cancellationToken);
        var result = await mediator.Send(new GetOrdersReportQuery(startDate, endDate, status), cancellationToken);
        return Ok(result);
    }

    [HttpGet("customers")]
    public async Task<ActionResult<CustomerReportDto>> GetCustomerReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated customer report.", cancellationToken);
        var result = await mediator.Send(new GetCustomerReportQuery(startDate, endDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<ActionResult<ProductReportDto>> GetProductReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated product performance report.", cancellationToken);
        var result = await mediator.Send(new GetProductReportQuery(startDate, endDate, categoryId, brandId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> GetInventoryReport(CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated inventory status report.", cancellationToken);
        var result = await mediator.Send(new GetInventoryReportQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<PaymentReportDto>> GetPaymentReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? method,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated payment methods report.", cancellationToken);
        var result = await mediator.Send(new GetPaymentReportQuery(startDate, endDate, method), cancellationToken);
        return Ok(result);
    }

    [HttpGet("coupons")]
    public async Task<ActionResult<CouponReportDto>> GetCouponReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated coupon campaigns report.", cancellationToken);
        var result = await mediator.Send(new GetCouponReportQuery(startDate, endDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("reviews")]
    public async Task<ActionResult<ReviewReportDto>> GetReviewReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportGenerated", "Admin generated customer reviews report.", cancellationToken);
        var result = await mediator.Send(new GetReviewReportQuery(startDate, endDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportReport(
        [FromQuery] string reportType,
        [FromQuery] string format,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        await LogAuditAsync("ReportExported", $"Admin exported {reportType} report in {format} format.", cancellationToken);
        var fileBytes = await mediator.Send(new ExportReportQuery(reportType, format, startDate, endDate), cancellationToken);

        var contentType = format.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "excel" => "application/vnd.ms-excel",
            _ => "text/csv"
        };

        var fileExtension = format.ToLowerInvariant() switch
        {
            "pdf" => "pdf",
            "excel" => "xls",
            _ => "csv"
        };

        var fileName = $"{reportType}_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";

        return File(fileBytes, contentType, fileName);
    }
}

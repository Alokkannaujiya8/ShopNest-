using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Reports;

public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public sealed record GetSalesReportQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? CategoryId,
    Guid? BrandId
) : IRequest<SalesReportDto>;

public sealed record GetRevenueReportQuery(
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<RevenueReportDto>;

public sealed record GetOrdersReportQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Status
) : IRequest<OrdersReportDto>;

public sealed record GetCustomerReportQuery(
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<CustomerReportDto>;

public sealed record GetProductReportQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? CategoryId,
    Guid? BrandId
) : IRequest<ProductReportDto>;

public sealed record GetInventoryReportQuery : IRequest<InventoryReportDto>;

public sealed record GetPaymentReportQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Method
) : IRequest<PaymentReportDto>;

public sealed record GetCouponReportQuery(
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<CouponReportDto>;

public sealed record GetReviewReportQuery(
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<ReviewReportDto>;

public sealed record ExportReportQuery(
    string ReportType,
    string Format,
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<byte[]>;

// Validators
public sealed class GetSalesReportQueryValidator : AbstractValidator<GetSalesReportQuery>
{
    public GetSalesReportQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate ?? DateTime.UtcNow)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to End date.");
    }
}

public sealed class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate ?? DateTime.UtcNow)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to End date.");
    }
}

public sealed class ExportReportQueryValidator : AbstractValidator<ExportReportQuery>
{
    public ExportReportQueryValidator()
    {
        RuleFor(x => x.ReportType)
            .NotEmpty()
            .WithMessage("Report type is required.");

        RuleFor(x => x.Format)
            .Must(x => x.Equals("pdf", StringComparison.OrdinalIgnoreCase) ||
                       x.Equals("excel", StringComparison.OrdinalIgnoreCase) ||
                       x.Equals("csv", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Export format must be either PDF, Excel, or CSV.");
    }
}

// Handlers
public sealed class ReportsHandlers(IReportingService reportingService) :
    IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>,
    IRequestHandler<GetSalesReportQuery, SalesReportDto>,
    IRequestHandler<GetRevenueReportQuery, RevenueReportDto>,
    IRequestHandler<GetOrdersReportQuery, OrdersReportDto>,
    IRequestHandler<GetCustomerReportQuery, CustomerReportDto>,
    IRequestHandler<GetProductReportQuery, ProductReportDto>,
    IRequestHandler<GetInventoryReportQuery, InventoryReportDto>,
    IRequestHandler<GetPaymentReportQuery, PaymentReportDto>,
    IRequestHandler<GetCouponReportQuery, CouponReportDto>,
    IRequestHandler<GetReviewReportQuery, ReviewReportDto>,
    IRequestHandler<ExportReportQuery, byte[]>
{
    public Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken) =>
        reportingService.GetDashboardSummaryAsync(cancellationToken);

    public Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetSalesReportAsync(request.StartDate, request.EndDate, request.CategoryId, request.BrandId, cancellationToken);

    public Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetRevenueReportAsync(request.StartDate, request.EndDate, cancellationToken);

    public Task<OrdersReportDto> Handle(GetOrdersReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetOrdersReportAsync(request.StartDate, request.EndDate, request.Status, cancellationToken);

    public Task<CustomerReportDto> Handle(GetCustomerReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetCustomerReportAsync(request.StartDate, request.EndDate, cancellationToken);

    public Task<ProductReportDto> Handle(GetProductReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetProductReportAsync(request.StartDate, request.EndDate, request.CategoryId, request.BrandId, cancellationToken);

    public Task<InventoryReportDto> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetInventoryReportAsync(cancellationToken);

    public Task<PaymentReportDto> Handle(GetPaymentReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetPaymentReportAsync(request.StartDate, request.EndDate, request.Method, cancellationToken);

    public Task<CouponReportDto> Handle(GetCouponReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetCouponReportAsync(request.StartDate, request.EndDate, cancellationToken);

    public Task<ReviewReportDto> Handle(GetReviewReportQuery request, CancellationToken cancellationToken) =>
        reportingService.GetReviewReportAsync(request.StartDate, request.EndDate, cancellationToken);

    public Task<byte[]> Handle(ExportReportQuery request, CancellationToken cancellationToken) =>
        reportingService.ExportReportAsync(request.ReportType, request.Format, request.StartDate, request.EndDate, cancellationToken);
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

// Queries
public record GetAuditLogsQuery(LogQueryRequest Request) : IRequest<PagedResult<AuditLogDto>>;
public record GetActivityLogsQuery(LogQueryRequest Request) : IRequest<PagedResult<ActivityLogDto>>;
public record GetErrorLogsQuery(LogQueryRequest Request) : IRequest<PagedResult<ErrorLogDto>>;
public record GetLoginHistoriesQuery(LogQueryRequest Request) : IRequest<PagedResult<LoginHistoryDto>>;
public record GetAuditDashboardSummaryQuery : IRequest<AuditDashboardSummaryDto>;

// Handlers
public sealed class AuditQueryHandlers : 
    IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>,
    IRequestHandler<GetActivityLogsQuery, PagedResult<ActivityLogDto>>,
    IRequestHandler<GetErrorLogsQuery, PagedResult<ErrorLogDto>>,
    IRequestHandler<GetLoginHistoriesQuery, PagedResult<LoginHistoryDto>>,
    IRequestHandler<GetAuditDashboardSummaryQuery, AuditDashboardSummaryDto>
{
    private readonly IAuditSearchService _searchService;

    public AuditQueryHandlers(IAuditSearchService searchService)
    {
        _searchService = searchService;
    }

    public Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        return _searchService.SearchAuditsAsync(request.Request, cancellationToken);
    }

    public Task<PagedResult<ActivityLogDto>> Handle(GetActivityLogsQuery request, CancellationToken cancellationToken)
    {
        return _searchService.SearchActivitiesAsync(request.Request, cancellationToken);
    }

    public Task<PagedResult<ErrorLogDto>> Handle(GetErrorLogsQuery request, CancellationToken cancellationToken)
    {
        return _searchService.SearchErrorsAsync(request.Request, cancellationToken);
    }

    public Task<PagedResult<LoginHistoryDto>> Handle(GetLoginHistoriesQuery request, CancellationToken cancellationToken)
    {
        return _searchService.SearchLoginsAsync(request.Request, cancellationToken);
    }

    public Task<AuditDashboardSummaryDto> Handle(GetAuditDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        return _searchService.GetDashboardSummaryAsync(cancellationToken);
    }
}

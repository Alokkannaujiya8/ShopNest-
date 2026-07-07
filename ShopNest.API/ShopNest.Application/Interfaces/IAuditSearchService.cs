using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IAuditSearchService
{
    Task<PagedResult<AuditLogDto>> SearchAuditsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ActivityLogDto>> SearchActivitiesAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ErrorLogDto>> SearchErrorsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<LoginHistoryDto>> SearchLoginsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<AuditDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
}

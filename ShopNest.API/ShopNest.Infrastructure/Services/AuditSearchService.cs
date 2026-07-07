using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class AuditSearchService : IAuditSearchService
{
    private readonly ShopNestDbContext _dbContext;

    public AuditSearchService(ShopNestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AuditLogDto>> SearchAuditsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogs
            .Include(x => x.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Action.Contains(request.SearchTerm) ||
                                     x.EntityName.Contains(request.SearchTerm) ||
                                     x.Description.Contains(request.SearchTerm) ||
                                     (x.User != null && x.User.Email!.Contains(request.SearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(x => x.EventType == request.Type);
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            query = query.Where(x => x.Module == request.Module);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.EndDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogDto(
                x.Id,
                x.UserId,
                x.User != null ? x.User.Email : null,
                x.Role,
                x.EventType,
                x.Module,
                x.EntityName,
                x.EntityId,
                x.Action,
                x.OldValues,
                x.NewValues,
                x.Description,
                x.IPAddress,
                x.Browser,
                x.Device,
                x.UserAgent,
                x.CorrelationId,
                x.RequestId,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PagedResult<ActivityLogDto>> SearchActivitiesAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ActivityLogs
            .Include(x => x.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.ActivityType.Contains(request.SearchTerm) ||
                                     x.Description.Contains(request.SearchTerm) ||
                                     (x.User != null && x.User.Email!.Contains(request.SearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(x => x.ActivityType == request.Type);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.EndDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ActivityLogDto(
                x.Id,
                x.UserId,
                x.User != null ? x.User.Email : null,
                x.ActivityType,
                x.Description,
                x.IPAddress,
                x.Browser,
                x.Device,
                x.UserAgent,
                x.OS,
                x.Country,
                x.City,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ActivityLogDto>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PagedResult<ErrorLogDto>> SearchErrorsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ErrorLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.ExceptionMessage.Contains(request.SearchTerm) ||
                                     x.RequestPath!.Contains(request.SearchTerm) ||
                                     x.StackTrace!.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Level))
        {
            query = query.Where(x => x.Severity == request.Level);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.EndDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ErrorLogDto(
                x.Id,
                x.ExceptionMessage,
                x.ExceptionType,
                x.StackTrace,
                x.Source,
                x.RequestPath,
                x.RequestMethod,
                x.QueryString,
                x.RequestBody,
                x.UserId,
                x.IPAddress,
                x.UserAgent,
                x.CorrelationId,
                x.RequestId,
                x.Severity,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ErrorLogDto>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PagedResult<LoginHistoryDto>> SearchLoginsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.LoginHistories
            .Include(x => x.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Email.Contains(request.SearchTerm) ||
                                     x.FailureReason!.Contains(request.SearchTerm));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.EndDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LoginHistoryDto(
                x.Id,
                x.Email,
                x.UserId,
                x.IsSuccess,
                x.FailureReason,
                x.IPAddress,
                x.Browser,
                x.Device,
                x.UserAgent,
                x.OS,
                x.Country,
                x.City,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<LoginHistoryDto>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<AuditDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var startOfToday = DateTime.UtcNow.Date;

        var totalActivities = await _dbContext.ActivityLogs.CountAsync(cancellationToken);
        
        var failedLoginsToday = await _dbContext.LoginHistories
            .CountAsync(x => !x.IsSuccess && x.CreatedAtUtc >= startOfToday, cancellationToken);

        var successLoginsToday = await _dbContext.LoginHistories
            .CountAsync(x => x.IsSuccess && x.CreatedAtUtc >= startOfToday, cancellationToken);

        var errorsToday = await _dbContext.ErrorLogs
            .CountAsync(x => x.CreatedAtUtc >= startOfToday, cancellationToken);

        var criticalErrorsToday = await _dbContext.ErrorLogs
            .CountAsync(x => x.Severity == "Critical" && x.CreatedAtUtc >= startOfToday, cancellationToken);

        var recentActivities = await _dbContext.ActivityLogs
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .Select(x => new ActivityLogDto(
                x.Id,
                x.UserId,
                x.User != null ? x.User.Email : null,
                x.ActivityType,
                x.Description,
                x.IPAddress,
                x.Browser,
                x.Device,
                x.UserAgent,
                x.OS,
                x.Country,
                x.City,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new AuditDashboardSummaryDto(
            totalActivities,
            failedLoginsToday,
            successLoginsToday,
            errorsToday,
            criticalErrorsToday,
            recentActivities
        );
    }
}

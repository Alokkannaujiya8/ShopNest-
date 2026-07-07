using System;
using System.Collections.Generic;

namespace ShopNest.Application.Dtos;

public record LogQueryRequest(
    string? SearchTerm = null,
    string? Type = null,
    string? Module = null,
    string? Level = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 10
);

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string? Role,
    string EventType,
    string Module,
    string EntityName,
    string EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    string Description,
    string? IPAddress,
    string? Browser,
    string? Device,
    string? UserAgent,
    string? CorrelationId,
    string? RequestId,
    DateTime CreatedAtUtc
);

public record ActivityLogDto(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string ActivityType,
    string Description,
    string? IPAddress,
    string? Browser,
    string? Device,
    string? UserAgent,
    string? OS,
    string? Country,
    string? City,
    DateTime CreatedAtUtc
);

public record LoginHistoryDto(
    Guid Id,
    string Email,
    Guid? UserId,
    bool IsSuccess,
    string? FailureReason,
    string? IPAddress,
    string? Browser,
    string? Device,
    string? UserAgent,
    string? OS,
    string? Country,
    string? City,
    DateTime CreatedAtUtc
);

public record ErrorLogDto(
    Guid Id,
    string ExceptionMessage,
    string? ExceptionType,
    string? StackTrace,
    string? Source,
    string? RequestPath,
    string? RequestMethod,
    string? QueryString,
    string? RequestBody,
    string? UserId,
    string? IPAddress,
    string? UserAgent,
    string? CorrelationId,
    string? RequestId,
    string Severity,
    DateTime CreatedAtUtc
);

public record ApplicationLogDto(
    Guid Id,
    string Message,
    string Level,
    string? Exception,
    string? Properties,
    string? CorrelationId,
    string? RequestId,
    DateTime CreatedAtUtc
);

public record AuditDashboardSummaryDto(
    long TotalActivities,
    long FailedLoginsToday,
    long SuccessLoginsToday,
    long ErrorsToday,
    long CriticalErrorsToday,
    List<ActivityLogDto> RecentActivities
);

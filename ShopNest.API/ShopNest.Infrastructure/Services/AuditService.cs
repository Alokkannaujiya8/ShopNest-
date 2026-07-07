using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly ShopNestDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ShopNestDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RecordActivityAsync(Guid? userId, string activityType, string description)
    {
        var ip = GetIPAddress();
        var userAgent = GetUserAgent();
        var (browser, device, os) = ParseUserAgent(userAgent);

        var log = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? GetCurrentUserId(),
            ActivityType = activityType,
            Description = description,
            IPAddress = ip,
            Browser = browser,
            Device = device,
            UserAgent = userAgent,
            OS = os,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = GetCurrentUsername()
        };

        _dbContext.ActivityLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RecordLoginAttemptAsync(string email, Guid? userId, bool isSuccess, string? failureReason = null)
    {
        var ip = GetIPAddress();
        var userAgent = GetUserAgent();
        var (browser, device, os) = ParseUserAgent(userAgent);

        var history = new LoginHistory
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserId = userId,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            IPAddress = ip,
            Browser = browser,
            Device = device,
            UserAgent = userAgent,
            OS = os,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _dbContext.LoginHistories.Add(history);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RecordErrorAsync(Exception exception, string? path = null, string? method = null, string? body = null, string severity = "Error")
    {
        var ip = GetIPAddress();
        var userAgent = GetUserAgent();
        var correlationId = GetCorrelationId();
        var requestId = GetRequestId();
        var userId = GetCurrentUserId()?.ToString();

        var error = new ErrorLog
        {
            Id = Guid.NewGuid(),
            ExceptionMessage = exception.Message,
            ExceptionType = exception.GetType().FullName,
            StackTrace = exception.StackTrace,
            Source = exception.Source,
            RequestPath = path,
            RequestMethod = method,
            RequestBody = body,
            UserId = userId,
            IPAddress = ip,
            UserAgent = userAgent,
            CorrelationId = correlationId,
            RequestId = requestId,
            Severity = severity,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = GetCurrentUsername()
        };

        _dbContext.ErrorLogs.Add(error);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RecordSystemLogAsync(string message, string level = "Information", string? exception = null, Dictionary<string, string>? properties = null)
    {
        var log = new ApplicationLog
        {
            Id = Guid.NewGuid(),
            Message = message,
            Level = level,
            Exception = exception,
            Properties = properties != null ? JsonSerializer.Serialize(properties) : null,
            CorrelationId = GetCorrelationId(),
            RequestId = GetRequestId(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _dbContext.ApplicationLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    // Helper Methods
    private Guid? GetCurrentUserId()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idClaim, out var guid))
            {
                return guid;
            }
        }
        catch { }
        return null;
    }

    private string GetCurrentUsername()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.FindFirst(ClaimTypes.Name)?.Value 
                       ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? "System";
            }
        }
        catch { }
        return "System";
    }

    private string? GetIPAddress()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
        catch { return null; }
    }

    private string? GetUserAgent()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
        }
        catch { return null; }
    }

    private string? GetCorrelationId()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["X-Correlation-Id"].ToString() 
                   ?? _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        }
        catch { return null; }
    }

    private string? GetRequestId()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.TraceIdentifier;
        }
        catch { return null; }
    }

    private (string Browser, string Device, string OS) ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return ("Unknown", "Unknown", "Unknown");

        var ua = userAgent.ToLower();
        var browser = "Other";
        var os = "Other";
        var device = "Desktop";

        if (ua.Contains("edg")) browser = "Edge";
        else if (ua.Contains("chrome")) browser = "Chrome";
        else if (ua.Contains("safari")) browser = "Safari";
        else if (ua.Contains("firefox")) browser = "Firefox";
        else if (ua.Contains("opera") || ua.Contains("opr")) browser = "Opera";

        if (ua.Contains("windows")) os = "Windows";
        else if (ua.Contains("android")) os = "Android";
        else if (ua.Contains("iphone") || ua.Contains("ipad")) os = "iOS";
        else if (ua.Contains("macintosh") || ua.Contains("mac os")) os = "macOS";
        else if (ua.Contains("linux")) os = "Linux";

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone")) device = "Mobile";
        else if (ua.Contains("ipad") || ua.Contains("tablet")) device = "Tablet";

        return (browser, device, os);
    }
}

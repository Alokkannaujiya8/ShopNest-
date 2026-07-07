using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNest.Application.Interfaces;

public interface IAuditService
{
    Task RecordActivityAsync(Guid? userId, string activityType, string description);
    Task RecordLoginAttemptAsync(string email, Guid? userId, bool isSuccess, string? failureReason = null);
    Task RecordErrorAsync(Exception exception, string? path = null, string? method = null, string? body = null, string severity = "Error");
    Task RecordSystemLogAsync(string message, string level = "Information", string? exception = null, Dictionary<string, string>? properties = null);
}

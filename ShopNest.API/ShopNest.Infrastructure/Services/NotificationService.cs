using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class NotificationService(
    ShopNestDbContext db,
    IEnumerable<INotificationProvider> providers,
    IHubContext<OrderHub> hubContext
) : INotificationService
{
    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        Guid? userId,
        bool? isRead,
        string? channel,
        string? priority,
        string? search,
        DateTime? startDate,
        DateTime? endDate,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Notifications
            .Include(x => x.User)
            .AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (isRead.HasValue)
        {
            query = query.Where(x => x.IsRead == isRead.Value);
        }

        if (!string.IsNullOrEmpty(channel))
        {
            query = query.Where(x => x.Channel == channel);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            query = query.Where(x => x.Priority == priority);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => (x.SentTime ?? x.CreatedAtUtc) >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => (x.SentTime ?? x.CreatedAtUtc) <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(x => 
                x.Title.ToLower().Contains(s) || 
                x.Message.ToLower().Contains(s) ||
                x.User.FullName.ToLower().Contains(s)
            );
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "oldest" => query.OrderBy(x => x.SentTime ?? x.CreatedAtUtc),
            "priority" => query.OrderByDescending(x => x.Priority == "High" ? 3 : x.Priority == "Medium" ? 2 : 1)
                               .ThenByDescending(x => x.SentTime ?? x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.SentTime ?? x.CreatedAtUtc)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<NotificationDto>(dtos, page, pageSize, total);
    }

    public async Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var item = await db.Notifications
            .Include(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        return item == null ? null : MapToDto(item);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Notifications
            .CountAsync(x => x.UserId == userId && !x.IsRead && !x.IsDeleted, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);

        if (notification is null) return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            
            // Audit Log
            db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "NotificationRead",
                EntityName = "Notification",
                EntityId = notificationId.ToString(),
                Details = "Notification marked as read."
            });

            await db.SaveChangesAsync(cancellationToken);

            // SignalR Update
            var count = await GetUnreadCountAsync(userId, cancellationToken);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("unreadCountUpdated", count, cancellationToken);
        }

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var unread = await db.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0) return true;

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "NotificationMarkedAllRead",
            EntityName = "Notification",
            EntityId = userId.ToString(),
            Details = $"Marked {unread.Count} notifications as read."
        });

        await db.SaveChangesAsync(cancellationToken);

        // SignalR Update
        await hubContext.Clients.Group(userId.ToString()).SendAsync("unreadCountUpdated", 0, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        if (notification is null) return false;

        if (notification.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this notification.");
        }

        notification.IsDeleted = true;
        notification.DeletedAtUtc = DateTime.UtcNow;
        notification.DeletedBy = userId.ToString();

        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "NotificationDeleted",
            EntityName = "Notification",
            EntityId = notificationId.ToString(),
            Details = "Notification soft deleted."
        });

        await db.SaveChangesAsync(cancellationToken);

        // SignalR Update
        var count = await GetUnreadCountAsync(notification.UserId, cancellationToken);
        await hubContext.Clients.Group(notification.UserId.ToString()).SendAsync("unreadCountUpdated", count, cancellationToken);
        return true;
    }

    public async Task<NotificationDto> SendManualNotificationAsync(SendManualNotificationRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync(new object[] { request.UserId }, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            NotificationType = request.NotificationType,
            Channel = request.Channel,
            Priority = request.Priority,
            Status = "Pending",
            SentTime = DateTime.UtcNow
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        await DispatchProviderAsync(notification, user.Email, user.MobileNumber, cancellationToken);

        // SignalR In-App trigger if Channel is InApp
        if (request.Channel.Equals("InApp", StringComparison.OrdinalIgnoreCase))
        {
            var dto = MapToDto(notification);
            await hubContext.Clients.Group(request.UserId.ToString()).SendAsync("notificationReceived", dto, cancellationToken);
            
            var count = await GetUnreadCountAsync(request.UserId, cancellationToken);
            await hubContext.Clients.Group(request.UserId.ToString()).SendAsync("unreadCountUpdated", count, cancellationToken);
        }

        return MapToDto(notification);
    }

    public async Task<bool> BroadcastNotificationAsync(BroadcastNotificationRequest request, CancellationToken cancellationToken)
    {
        var users = await db.Users.Where(x => !x.IsDeleted && x.IsActive).ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var notification = new Notification
            {
                UserId = user.Id,
                Title = request.Title,
                Message = request.Message,
                NotificationType = request.NotificationType,
                Channel = request.Channel,
                Priority = request.Priority,
                Status = "Pending",
                SentTime = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync(cancellationToken);

            await DispatchProviderAsync(notification, user.Email, user.MobileNumber, cancellationToken);

            if (request.Channel.Equals("InApp", StringComparison.OrdinalIgnoreCase))
            {
                var dto = MapToDto(notification);
                await hubContext.Clients.Group(user.Id.ToString()).SendAsync("notificationReceived", dto, cancellationToken);
                
                var count = await GetUnreadCountAsync(user.Id, cancellationToken);
                await hubContext.Clients.Group(user.Id.ToString()).SendAsync("unreadCountUpdated", count, cancellationToken);
            }
        }

        return true;
    }

    public async Task<bool> SendTemplatedNotificationAsync(
        Guid userId,
        string templateCode,
        Dictionary<string, string> templateVariables,
        string? relatedEntity,
        string? relatedEntityId,
        CancellationToken cancellationToken)
    {
        var template = await db.NotificationTemplates
            .FirstOrDefaultAsync(x => x.Code == templateCode, cancellationToken);

        var user = await db.Users.FindAsync(new object[] { userId }, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        // Inject default user variables if not present
        if (!templateVariables.ContainsKey("FullName"))
        {
            templateVariables["FullName"] = user.FullName;
        }

        string parsedSubject, parsedBody, channel;
        if (template == null)
        {
            channel = "InApp";
            if (templateCode == "OtpEmail")
            {
                parsedSubject = "ShopNest OTP Verification Code";
                parsedBody = $"Dear {user.FullName}, your OTP verification code is '{templateVariables.GetValueOrDefault("Otp", "000000")}'";
                channel = "Email";
            }
            else
            {
                parsedSubject = $"System Notification: {templateCode}";
                parsedBody = $"Notification related to {relatedEntity} {relatedEntityId}.";
            }
        }
        else
        {
            parsedSubject = ReplaceTemplateVariables(template.Subject, templateVariables);
            parsedBody = ReplaceTemplateVariables(template.Body, templateVariables);
            channel = template.Channel;
        }

        var notification = new Notification
        {
            UserId = userId,
            Title = parsedSubject,
            Message = parsedBody,
            NotificationType = templateCode.Contains("Failure") || templateCode.Contains("Failed") || templateCode.Contains("Cancel") ? "Error" : "Success",
            Channel = channel,
            Priority = "Medium",
            Status = "Pending",
            RelatedEntity = relatedEntity ?? string.Empty,
            RelatedEntityId = relatedEntityId ?? string.Empty,
            SentTime = DateTime.UtcNow
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        var dispatchSuccess = await DispatchProviderAsync(notification, user.Email, user.MobileNumber, cancellationToken);

        // Also duplicate as InApp so that the customer has a persistent record in their notification drawer!
        if (!channel.Equals("InApp", StringComparison.OrdinalIgnoreCase))
        {
            var inAppNotification = new Notification
            {
                UserId = userId,
                Title = parsedSubject,
                Message = parsedBody,
                NotificationType = notification.NotificationType,
                Channel = "InApp",
                Priority = "Medium",
                Status = "Sent",
                RelatedEntity = relatedEntity ?? string.Empty,
                RelatedEntityId = relatedEntityId ?? string.Empty,
                SentTime = DateTime.UtcNow
            };
            db.Notifications.Add(inAppNotification);
            await db.SaveChangesAsync(cancellationToken);

            // Trigger SignalR in-app
            var dto = MapToDto(inAppNotification);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("notificationReceived", dto, cancellationToken);
            
            var count = await GetUnreadCountAsync(userId, cancellationToken);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("unreadCountUpdated", count, cancellationToken);
        }
        else
        {
            // If the primary channel is InApp, trigger SignalR
            var dto = MapToDto(notification);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("notificationReceived", dto, cancellationToken);

            var count = await GetUnreadCountAsync(userId, cancellationToken);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("unreadCountUpdated", count, cancellationToken);
        }

        return dispatchSuccess;
    }

    // Template Actions
    public async Task<List<NotificationTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken)
    {
        var list = await db.NotificationTemplates.AsNoTracking().ToListAsync(cancellationToken);
        return list.Select(MapTemplateToDto).ToList();
    }

    public async Task<NotificationTemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var existing = await db.NotificationTemplates.AnyAsync(x => x.Code == request.Code, cancellationToken);
        if (existing) throw new InvalidOperationException($"Template code '{request.Code}' already exists.");

        var t = new NotificationTemplate
        {
            Code = request.Code,
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body,
            Channel = request.Channel
        };

        db.NotificationTemplates.Add(t);
        await db.SaveChangesAsync(cancellationToken);
        return MapTemplateToDto(t);
    }

    public async Task<NotificationTemplateDto> UpdateTemplateAsync(string code, UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        var t = await db.NotificationTemplates.FirstOrDefaultAsync(x => x.Code == code, cancellationToken)
            ?? throw new KeyNotFoundException("Template not found.");

        t.Name = request.Name;
        t.Subject = request.Subject;
        t.Body = request.Body;
        t.Channel = request.Channel;

        await db.SaveChangesAsync(cancellationToken);
        return MapTemplateToDto(t);
    }

    // Logs Actions
    public async Task<PagedResult<NotificationLogDto>> GetNotificationLogsAsync(Guid? notificationId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.NotificationLogs.AsNoTracking();
        if (notificationId.HasValue)
        {
            query = query.Where(x => x.NotificationId == notificationId.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.SentAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapLogToDto).ToList();
        return new PagedResult<NotificationLogDto>(dtos, page, pageSize, total);
    }

    // Helper utilities
    private async Task<bool> DispatchProviderAsync(Notification notification, string email, string phone, CancellationToken cancellationToken)
    {
        if (notification.Channel.Equals("InApp", StringComparison.OrdinalIgnoreCase))
        {
            notification.Status = "Sent";
            return true;
        }

        var provider = providers.FirstOrDefault(x => x.Channel.Equals(notification.Channel, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            // Fail and log
            var err = $"No provider registered for channel '{notification.Channel}'.";
            notification.Status = "Failed";
            db.NotificationLogs.Add(new NotificationLog
            {
                NotificationId = notification.Id,
                Channel = notification.Channel,
                Recipient = notification.Channel == "Email" ? email : phone,
                Status = "Failed",
                ErrorMessage = err
            });
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }

        var recipient = notification.Channel == "Email" ? email : phone;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            var err = $"Recipient address/number was empty for channel '{notification.Channel}'.";
            notification.Status = "Failed";
            db.NotificationLogs.Add(new NotificationLog
            {
                NotificationId = notification.Id,
                Channel = notification.Channel,
                Recipient = string.Empty,
                Status = "Failed",
                ErrorMessage = err
            });
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }

        try
        {
            var sent = await provider.SendAsync(recipient, notification.Title, notification.Message, cancellationToken);
            notification.Status = sent ? "Sent" : "Failed";

            db.NotificationLogs.Add(new NotificationLog
            {
                NotificationId = notification.Id,
                Channel = notification.Channel,
                Recipient = recipient,
                Status = sent ? "Sent" : "Failed",
                ErrorMessage = sent ? string.Empty : "Provider SendAsync returned false."
            });
        }
        catch (Exception ex)
        {
            notification.Status = "Failed";
            db.NotificationLogs.Add(new NotificationLog
            {
                NotificationId = notification.Id,
                Channel = notification.Channel,
                Recipient = recipient,
                Status = "Failed",
                ErrorMessage = ex.Message
            });
        }

        // Save delivery status audit log
        db.AuditLogs.Add(new AuditLog
        {
            UserId = notification.UserId,
            Action = "NotificationSent",
            EntityName = "Notification",
            EntityId = notification.Id.ToString(),
            Details = $"Dispatched channel: {notification.Channel}. Delivery Status: {notification.Status}"
        });

        await db.SaveChangesAsync(cancellationToken);
        return notification.Status == "Sent";
    }

    private static string ReplaceTemplateVariables(string text, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(text)) return text;
        foreach (var (key, value) in variables)
        {
            text = text.Replace($"{{{{{key}}}}}", value);
        }
        return text;
    }

    private static NotificationDto MapToDto(Notification n) => new(
        n.Id,
        n.UserId,
        n.User?.FullName ?? "Unknown",
        n.Title,
        n.Message,
        n.NotificationType,
        n.Channel,
        n.Priority,
        n.Status,
        n.RelatedEntity,
        n.RelatedEntityId,
        n.IsRead,
        n.SentTime
    );

    private static NotificationTemplateDto MapTemplateToDto(NotificationTemplate t) => new(
        t.Id,
        t.Code,
        t.Name,
        t.Subject,
        t.Body,
        t.Channel
    );

    private static NotificationLogDto MapLogToDto(NotificationLog l) => new(
        l.Id,
        l.NotificationId,
        l.Channel,
        l.Recipient,
        l.Status,
        l.ErrorMessage,
        l.SentAtUtc
    );
}

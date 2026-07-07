using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface INotificationService
{
    // Fetching / User operations
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(
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
        CancellationToken cancellationToken);

    Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId, CancellationToken cancellationToken);
    
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
    
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken);
    
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, bool isAdmin, CancellationToken cancellationToken);

    // Direct Sending (Manual / Admin)
    Task<NotificationDto> SendManualNotificationAsync(SendManualNotificationRequest request, CancellationToken cancellationToken);
    
    Task<bool> BroadcastNotificationAsync(BroadcastNotificationRequest request, CancellationToken cancellationToken);

    // Automated Templated Notification Dispatcher (Main integration method used across other modules)
    Task<bool> SendTemplatedNotificationAsync(
        Guid userId,
        string templateCode,
        Dictionary<string, string> templateVariables,
        string? relatedEntity,
        string? relatedEntityId,
        CancellationToken cancellationToken);

    // Templates Management
    Task<List<NotificationTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken);
    
    Task<NotificationTemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken);
    
    Task<NotificationTemplateDto> UpdateTemplateAsync(string code, UpdateTemplateRequest request, CancellationToken cancellationToken);

    // Audit logs
    Task<PagedResult<NotificationLogDto>> GetNotificationLogsAsync(Guid? notificationId, int page, int pageSize, CancellationToken cancellationToken);
}

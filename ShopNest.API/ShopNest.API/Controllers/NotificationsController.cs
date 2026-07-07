using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] string? channel,
        [FromQuery] string? priority,
        [FromQuery] string? search,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = isAdmin ? (Guid?)null : User.UserId();

        var result = await notificationService.GetNotificationsAsync(
            userId,
            isRead,
            channel,
            priority,
            search,
            startDate,
            endDate,
            sortBy,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> GetNotificationById(Guid id, CancellationToken cancellationToken)
    {
        var item = await notificationService.GetNotificationByIdAsync(id, cancellationToken);
        if (item is null) return NotFound();

        // Security: Users can access only their own notifications
        var isAdmin = User.IsInRole("Admin");
        if (item.UserId != User.UserId() && !isAdmin)
        {
            return Forbid();
        }

        return Ok(item);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var count = await notificationService.GetUnreadCountAsync(User.UserId(), cancellationToken);
        return Ok(count);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var success = await notificationService.MarkAsReadAsync(id, User.UserId(), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await notificationService.MarkAllAsReadAsync(User.UserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteNotification(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var success = await notificationService.DeleteNotificationAsync(id, User.UserId(), isAdmin, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    // Admin APIs
    [Authorize(Roles = "Admin")]
    [HttpPost("send")]
    public async Task<ActionResult<NotificationDto>> SendManualNotification([FromBody] SendManualNotificationRequest request, CancellationToken cancellationToken)
    {
        var item = await notificationService.SendManualNotificationAsync(request, cancellationToken);
        return Ok(item);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("broadcast")]
    public async Task<ActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequest request, CancellationToken cancellationToken)
    {
        await notificationService.BroadcastNotificationAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("logs")]
    public async Task<ActionResult<PagedResult<NotificationLogDto>>> GetLogs(
        [FromQuery] Guid? notificationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await notificationService.GetNotificationLogsAsync(notificationId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("templates")]
    public async Task<ActionResult<List<NotificationTemplateDto>>> GetTemplates(CancellationToken cancellationToken)
    {
        var list = await notificationService.GetTemplatesAsync(cancellationToken);
        return Ok(list);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("templates")]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate([FromBody] CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var item = await notificationService.CreateTemplateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTemplates), item);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("templates/{code}")]
    public async Task<ActionResult<NotificationTemplateDto>> UpdateTemplate(string code, [FromBody] UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        var item = await notificationService.UpdateTemplateAsync(code, request, cancellationToken);
        return Ok(item);
    }
}

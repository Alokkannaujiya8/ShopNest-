using System;
using System.Collections.Generic;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "Information"; // Information, Success, Warning, Error, Promotion
    public string Channel { get; set; } = "InApp"; // Email, InApp, SMS, Push, WhatsApp, Telegram
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    public string Status { get; set; } = "Sent"; // Pending, Sent, Failed
    public string RelatedEntity { get; set; } = string.Empty;
    public string RelatedEntityId { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? SentTime { get; set; } = DateTime.UtcNow;

    public List<NotificationLog> Logs { get; set; } = [];
}

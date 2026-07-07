using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class NotificationLog : BaseEntity
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent"; // Sent, Failed
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}

using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class NotificationTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Channel { get; set; } = "Email"; // Email, SMS, Push, WhatsApp, etc.
}

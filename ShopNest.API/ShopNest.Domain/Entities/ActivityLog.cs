using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string ActivityType { get; set; } = string.Empty; // e.g. Login, Logout, ProfileUpdate, CartAdd
    public string Description { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? Browser { get; set; }
    public string? Device { get; set; }
    public string? UserAgent { get; set; }
    public string? OS { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime? LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public double? SessionDurationSeconds { get; set; }
}

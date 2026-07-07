using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class LoginHistory : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; } // e.g. InvalidPassword, AccountLocked, EmailNotVerified
    public string? IPAddress { get; set; }
    public string? Browser { get; set; }
    public string? Device { get; set; }
    public string? UserAgent { get; set; }
    public string? OS { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}

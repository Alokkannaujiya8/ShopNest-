using ShopNest.Domain.Common;
using ShopNest.Domain.Enums;

namespace ShopNest.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationOtpHash { get; set; }
    public DateTime? EmailVerificationOtpExpiresAtUtc { get; set; }
    public int EmailVerificationOtpRetryCount { get; set; } = 0;
    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public string? PasswordResetOtpHash { get; set; }
    public DateTime? PasswordResetOtpExpiresAtUtc { get; set; }
    public Cart? Cart { get; set; }
    public List<Order> Orders { get; set; } = [];
    public UserProfile? Profile { get; set; }
    public List<UserAddress> Addresses { get; set; } = [];
    public List<WishlistItem> WishlistItems { get; set; } = [];
    public List<AppUserRole> UserRoles { get; set; } = [];
    public List<OtpVerification> OtpVerifications { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<CouponUsage> CouponUsages { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
    public List<AuditLog> AuditLogs { get; set; } = [];

    // Lockout & Administration properties
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public int LoginCount { get; set; } = 0;
    public bool ForcePasswordChange { get; set; } = false;
    public bool MobileVerified { get; set; } = false;
    public List<UserPermission> UserPermissions { get; set; } = [];
}

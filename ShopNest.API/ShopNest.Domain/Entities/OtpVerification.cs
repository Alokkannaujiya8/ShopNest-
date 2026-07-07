using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class OtpVerification : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string OtpHash { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // EmailVerification, PasswordReset
    public DateTime ExpiresAtUtc { get; set; }
    public int RetryCount { get; set; } = 0;
    public bool IsUsed { get; set; } = false;
}

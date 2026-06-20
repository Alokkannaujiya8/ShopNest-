using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}

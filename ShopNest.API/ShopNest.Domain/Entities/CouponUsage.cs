using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
}

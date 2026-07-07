using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty; // Percentage, FixedAmount
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public int UsageLimit { get; set; } = 0;
    public int UsageCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public List<CouponUsage> Usages { get; set; } = [];
}

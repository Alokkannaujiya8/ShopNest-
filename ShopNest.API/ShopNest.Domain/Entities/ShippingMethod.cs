using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ShippingMethod : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int EstimatedDays { get; set; }
    public bool IsActive { get; set; } = true;
}

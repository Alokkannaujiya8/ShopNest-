using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Courier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string EstimatedDeliveryTime { get; set; } = "3-5 Business Days";
}

using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class OrderTracking : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string CourierPartner { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

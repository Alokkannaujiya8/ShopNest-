using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ShipmentTracking : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

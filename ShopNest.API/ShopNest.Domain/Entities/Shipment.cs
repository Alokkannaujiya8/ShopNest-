using System;
using System.Collections.Generic;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Shipment : BaseEntity
{
    public string ShipmentNumber { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? CourierId { get; set; }
    public Courier? Courier { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public DateTime? ShipmentDate { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal ShippingCharges { get; set; }
    public string DeliveryInstructions { get; set; } = string.Empty;
    public string Status { get; set; } = "Shipment Created";
    public string Notes { get; set; } = string.Empty;
    public List<ShipmentTracking> TrackingHistory { get; set; } = [];
}

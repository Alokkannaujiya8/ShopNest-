using ShopNest.Domain.Common;
using ShopNest.Domain.Enums;

namespace ShopNest.Domain.Entities;

public sealed class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CourierPartner { get; set; }
    public string? TrackingNumber { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public string? OrderNotes { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public OrderShippingAddress? ShippingAddressDetails { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public Payment? Payment { get; set; }
    public CouponUsage? CouponUsage { get; set; }
    public List<OrderStatusHistory> StatusHistory { get; set; } = [];
    public List<OrderTracking> TrackingUpdates { get; set; } = [];
    public List<ReturnRequest> ReturnRequests { get; set; } = [];
    public List<Shipment> Shipments { get; set; } = [];
}

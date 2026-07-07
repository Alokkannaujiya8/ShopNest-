using System;
using ShopNest.Domain.Common;
using ShopNest.Domain.Enums;

namespace ShopNest.Domain.Entities;

public sealed class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public string Note { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "System";
}

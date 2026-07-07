using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ReturnRequest : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string? AdminNotes { get; set; }
}

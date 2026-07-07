using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
}

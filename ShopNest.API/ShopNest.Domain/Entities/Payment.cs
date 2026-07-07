using ShopNest.Domain.Common;
using ShopNest.Domain.Enums;

namespace ShopNest.Domain.Entities;

public sealed class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string Provider { get; set; } = string.Empty;
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string ProviderOrderId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "inr";
    public List<PaymentTransaction> Transactions { get; set; } = [];
}

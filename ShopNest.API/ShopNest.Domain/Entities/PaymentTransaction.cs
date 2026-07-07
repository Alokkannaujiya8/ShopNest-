using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public string GatewayTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Failed, Pending
    public string? RawResponse { get; set; }
}

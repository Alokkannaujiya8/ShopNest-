using System;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Payments;

public sealed class CodPaymentProvider : IPaymentProvider
{
    public string ProviderName => "CashOnDelivery";

    public Task<PaymentSessionResponse> InitializePaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        var id = $"cod_{Guid.NewGuid():N}";
        payment.ProviderPaymentId = id;
        payment.ProviderOrderId = id;
        return Task.FromResult(new PaymentSessionResponse(payment.Id, ProviderName, id, id));
    }

    public Task<bool> VerifyPaymentAsync(Payment payment, string transactionId, CancellationToken cancellationToken)
    {
        // COD payments are validated successfully, with final balance completed on delivery.
        return Task.FromResult(true);
    }

    public Task<bool> RefundPaymentAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken)
    {
        // COD refunds are processed manually or via bank transfer.
        return Task.FromResult(true);
    }
}

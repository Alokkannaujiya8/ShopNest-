using System;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Dtos;
using ShopNest.Domain.Entities;

namespace ShopNest.Application.Interfaces;

public interface IPaymentProvider
{
    string ProviderName { get; }
    Task<PaymentSessionResponse> InitializePaymentAsync(Payment payment, CancellationToken cancellationToken);
    Task<bool> VerifyPaymentAsync(Payment payment, string transactionId, CancellationToken cancellationToken);
    Task<bool> RefundPaymentAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken);
}

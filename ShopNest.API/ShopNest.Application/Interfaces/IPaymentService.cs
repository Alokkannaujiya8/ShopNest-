using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentSessionResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken);
    Task HandleWebhookAsync(string provider, string payload, string? signature, CancellationToken cancellationToken);
    Task<bool> CompletePaymentAsync(CompletePaymentRequest request, CancellationToken cancellationToken);

    // Advanced features
    Task<PaymentSessionResponse> InitializePaymentAsync(Guid orderId, string provider, string currency, CancellationToken cancellationToken);
    Task<bool> VerifyPaymentAsync(Guid paymentId, string transactionId, CancellationToken cancellationToken);
    Task<bool> RefundPaymentAsync(Guid paymentId, decimal amount, string reason, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentDto>> GetPaymentHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task<PaymentDto?> GetPaymentDetailsAsync(Guid paymentId, CancellationToken cancellationToken);
}

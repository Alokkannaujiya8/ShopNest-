using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentSessionResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken);
    Task HandleWebhookAsync(string provider, string payload, string? signature, CancellationToken cancellationToken);
    Task<bool> CompletePaymentAsync(CompletePaymentRequest request, CancellationToken cancellationToken);
}

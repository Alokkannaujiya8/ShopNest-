using ShopNest.Domain.Enums;

namespace ShopNest.Application.Dtos;

public sealed record PaymentDto(Guid Id, string Provider, string ProviderPaymentId, string ProviderOrderId, PaymentStatus Status, decimal Amount, string Currency);
public sealed record CreatePaymentRequest(Guid OrderId, string Provider = "stripe", string Currency = "inr");
public sealed record PaymentSessionResponse(Guid PaymentId, string Provider, string ClientSecret, string ProviderOrderId);
public sealed record CompletePaymentRequest(Guid PaymentId, PaymentStatus Status);

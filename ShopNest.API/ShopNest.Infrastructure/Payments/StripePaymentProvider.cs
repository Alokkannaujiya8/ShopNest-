using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Settings;
using Stripe;

namespace ShopNest.Infrastructure.Payments;

public sealed class StripePaymentProvider(IOptions<PaymentSettings> options) : IPaymentProvider
{
    public string ProviderName => "Stripe";

    public async Task<PaymentSessionResponse> InitializePaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        StripeConfiguration.ApiKey = options.Value.StripeSecretKey;

        if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
        {
            // Fallback for local sandbox/testing if key not configured
            var fallbackId = $"stripe_mock_{Guid.NewGuid():N}";
            return new PaymentSessionResponse(payment.Id, ProviderName, "mock_client_secret", fallbackId);
        }

        var service = new PaymentIntentService();
        var intentOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(payment.Amount * 100), // convert to cents
            Currency = payment.Currency.ToLowerInvariant(),
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = payment.OrderId.ToString(),
                ["paymentId"] = payment.Id.ToString()
            },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };

        var intent = await service.CreateAsync(intentOptions, cancellationToken: cancellationToken);

        payment.ProviderPaymentId = intent.Id;
        payment.ProviderOrderId = intent.Id;

        return new PaymentSessionResponse(payment.Id, ProviderName, intent.ClientSecret, intent.Id);
    }

    public async Task<bool> VerifyPaymentAsync(Payment payment, string transactionId, CancellationToken cancellationToken)
    {
        StripeConfiguration.ApiKey = options.Value.StripeSecretKey;
        if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
        {
            return true; // Mock success
        }

        var service = new PaymentIntentService();
        var intent = await service.GetAsync(payment.ProviderPaymentId, cancellationToken: cancellationToken);
        return intent.Status == "succeeded";
    }

    public async Task<bool> RefundPaymentAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken)
    {
        StripeConfiguration.ApiKey = options.Value.StripeSecretKey;
        if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
        {
            return true; // Mock success
        }

        var service = new RefundService();
        var refundOptions = new RefundCreateOptions
        {
            PaymentIntent = payment.ProviderPaymentId,
            Amount = (long)(amount * 100),
            Reason = "requested_by_customer",
            Metadata = new Dictionary<string, string>
            {
                ["paymentId"] = payment.Id.ToString(),
                ["reason"] = reason
            }
        };

        var refund = await service.CreateAsync(refundOptions, cancellationToken: cancellationToken);
        return refund.Status == "succeeded";
    }
}

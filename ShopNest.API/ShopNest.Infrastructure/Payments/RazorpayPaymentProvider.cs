using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Settings;

namespace ShopNest.Infrastructure.Payments;

public sealed class RazorpayPaymentProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentSettings> options
) : IPaymentProvider
{
    public string ProviderName => "Razorpay";

    public async Task<PaymentSessionResponse> InitializePaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        var keyId = options.Value.RazorpayKeyId;
        var keySecret = options.Value.RazorpayKeySecret;

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            var fallbackId = $"rzp_mock_{Guid.NewGuid():N}";
            return new PaymentSessionResponse(payment.Id, ProviderName, "mock_razorpay_secret", fallbackId);
        }

        var client = httpClientFactory.CreateClient();
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var requestBody = new
        {
            amount = (int)(payment.Amount * 100), // in paise
            currency = payment.Currency.ToUpperInvariant(),
            receipt = payment.OrderId.ToString(),
            notes = new { paymentId = payment.Id.ToString() }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.razorpay.com/v1/orders", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Razorpay order creation failed: {err}");
        }

        var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(resJson);
        var orderId = doc.RootElement.GetProperty("id").GetString()!;

        payment.ProviderOrderId = orderId;
        payment.ProviderPaymentId = orderId;

        return new PaymentSessionResponse(payment.Id, ProviderName, orderId, orderId);
    }

    public async Task<bool> VerifyPaymentAsync(Payment payment, string transactionId, CancellationToken cancellationToken)
    {
        // For Razorpay, payment verification typically checks the signature matching the order & payment ID.
        // We can check the Razorpay payment status via GET v1/payments/{payment_id}
        var keyId = options.Value.RazorpayKeyId;
        var keySecret = options.Value.RazorpayKeySecret;

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            return true; // Mock success
        }

        var client = httpClientFactory.CreateClient();
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var response = await client.GetAsync($"https://api.razorpay.com/v1/payments/{transactionId}", cancellationToken);
        if (!response.IsSuccessStatusCode) return false;

        var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(resJson);
        var status = doc.RootElement.GetProperty("status").GetString();

        return status == "captured" || status == "authorized";
    }

    public async Task<bool> RefundPaymentAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken)
    {
        var keyId = options.Value.RazorpayKeyId;
        var keySecret = options.Value.RazorpayKeySecret;

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            return true; // Mock success
        }

        var client = httpClientFactory.CreateClient();
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var requestBody = new
        {
            amount = (int)(amount * 100),
            notes = new { reason }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // razorpay refunds require a payment transaction ID (ProviderPaymentId)
        var response = await client.PostAsync($"https://api.razorpay.com/v1/payments/{payment.ProviderPaymentId}/refund", content, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

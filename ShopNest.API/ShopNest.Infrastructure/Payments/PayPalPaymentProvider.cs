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

public sealed class PayPalPaymentProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentSettings> options
) : IPaymentProvider
{
    public string ProviderName => "PayPal";

    private async Task<string> GetAccessTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var clientId = options.Value.PaypalClientId;
        var clientSecret = options.Value.PaypalClientSecret;

        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api-m.sandbox.paypal.com/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to obtain PayPal access token.");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<PaymentSessionResponse> InitializePaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        var clientId = options.Value.PaypalClientId;
        var clientSecret = options.Value.PaypalClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            var fallbackId = $"paypal_mock_{Guid.NewGuid():N}";
            return new PaymentSessionResponse(payment.Id, ProviderName, "mock_paypal_approval_url", fallbackId);
        }

        var client = httpClientFactory.CreateClient();
        var token = await GetAccessTokenAsync(client, cancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestBody = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    amount = new
                    {
                        currency_code = payment.Currency.ToUpperInvariant(),
                        value = payment.Amount.ToString("F2")
                    },
                    reference_id = payment.OrderId.ToString()
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api-m.sandbox.paypal.com/v2/checkout/orders", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"PayPal order creation failed: {err}");
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
        var clientId = options.Value.PaypalClientId;
        var clientSecret = options.Value.PaypalClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return true; // Mock success
        }

        var client = httpClientFactory.CreateClient();
        var token = await GetAccessTokenAsync(client, cancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"https://api-m.sandbox.paypal.com/v2/checkout/orders/{payment.ProviderOrderId}/capture", null, cancellationToken);
        if (!response.IsSuccessStatusCode) return false;

        var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(resJson);
        var status = doc.RootElement.GetProperty("status").GetString();

        return status == "COMPLETED";
    }

    public async Task<bool> RefundPaymentAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken)
    {
        var clientId = options.Value.PaypalClientId;
        var clientSecret = options.Value.PaypalClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return true; // Mock success
        }

        var client = httpClientFactory.CreateClient();
        var token = await GetAccessTokenAsync(client, cancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestBody = new
        {
            amount = new
            {
                value = amount.ToString("F2"),
                currency_code = payment.Currency.ToUpperInvariant()
            },
            note_to_payer = reason
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Paypal refunds require capturing ID
        var response = await client.PostAsync($"https://api-m.sandbox.paypal.com/v2/payments/captures/{payment.ProviderPaymentId}/refund", content, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

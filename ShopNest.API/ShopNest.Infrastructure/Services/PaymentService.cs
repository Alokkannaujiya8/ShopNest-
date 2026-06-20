using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;
using ShopNest.Infrastructure.Settings;
using Stripe;

namespace ShopNest.Infrastructure.Services;

public sealed class PaymentService(
    ShopNestDbContext db, 
    IOptions<PaymentSettings> options,
    IHubContext<OrderHub> hub) : IPaymentService
{
    public async Task<PaymentSessionResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(x => x.Payment).FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");

        if (order.Payment is not null && order.Payment.Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Order already paid.");
        }

        var provider = request.Provider.Trim().ToLowerInvariant();
        var payment = order.Payment ?? new Payment
        {
            OrderId = order.Id,
            Provider = provider,
            Amount = order.TotalAmount,
            Currency = request.Currency.ToLowerInvariant()
        };

        if (provider == "stripe")
        {
            StripeConfiguration.ApiKey = options.Value.StripeSecretKey;
            if (!string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
            {
                var intent = await new PaymentIntentService().CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = (long)(order.TotalAmount * 100),
                    Currency = payment.Currency,
                    Metadata = new Dictionary<string, string> { ["orderId"] = order.Id.ToString() },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
                }, cancellationToken: cancellationToken);

                payment.ProviderPaymentId = intent.Id;
                payment.ProviderOrderId = intent.Id;
                if (order.Payment is null) db.Payments.Add(payment);
                await db.SaveChangesAsync(cancellationToken);
                return new PaymentSessionResponse(payment.Id, provider, intent.ClientSecret, intent.Id);
            }
        }

        payment.ProviderOrderId = $"local_{Guid.NewGuid():N}";
        if (order.Payment is null) db.Payments.Add(payment);
        await db.SaveChangesAsync(cancellationToken);
        return new PaymentSessionResponse(payment.Id, provider, payment.ProviderOrderId, payment.ProviderOrderId);
    }

    public async Task<bool> CompletePaymentAsync(CompletePaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.Include(x => x.Order).ThenInclude(x => x.Items).FirstOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken);
        if (payment is null) return false;
        await MarkPaymentAsync(payment, request.Status, cancellationToken);
        return true;
    }

    public async Task HandleWebhookAsync(string provider, string payload, string? signature, CancellationToken cancellationToken)
    {
        provider = provider.Trim().ToLowerInvariant();
        if (provider == "stripe" && !string.IsNullOrWhiteSpace(options.Value.StripeWebhookSecret))
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, options.Value.StripeWebhookSecret);
            if (stripeEvent.Type == Events.PaymentIntentSucceeded && stripeEvent.Data.Object is PaymentIntent intent)
            {
                var payment = await db.Payments.Include(x => x.Order).ThenInclude(x => x.Items).FirstOrDefaultAsync(x => x.ProviderPaymentId == intent.Id || x.ProviderOrderId == intent.Id, cancellationToken);
                if (payment is not null) await MarkPaymentAsync(payment, PaymentStatus.Succeeded, cancellationToken);
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed && stripeEvent.Data.Object is PaymentIntent failed)
            {
                var payment = await db.Payments.Include(x => x.Order).ThenInclude(x => x.Items).FirstOrDefaultAsync(x => x.ProviderPaymentId == failed.Id || x.ProviderOrderId == failed.Id, cancellationToken);
                if (payment is not null) await MarkPaymentAsync(payment, PaymentStatus.Failed, cancellationToken);
            }
        }
    }

    private async Task MarkPaymentAsync(Payment payment, PaymentStatus status, CancellationToken cancellationToken)
    {
        payment.Status = status;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        // Notify client and admin
        var itemsDto = payment.Order.Items.Select(x => new OrderItemDto(x.ProductId, x.ProductName, x.UnitPrice, x.Quantity, x.UnitPrice * x.Quantity)).ToList();
        var paymentDto = new PaymentDto(payment.Id, payment.Provider, payment.ProviderPaymentId, payment.ProviderOrderId, payment.Status, payment.Amount, payment.Currency);
        var orderDto = new OrderDto(payment.Order.Id, payment.Order.OrderNumber, payment.Order.Status, payment.Order.TotalAmount, payment.Order.ShippingAddress, itemsDto, paymentDto);

        await hub.Clients.Group(payment.Order.UserId.ToString()).SendAsync("orderStatusChanged", orderDto, cancellationToken);

        if (status == PaymentStatus.Succeeded)
        {
            // Customer notification: Order Confirmed
            await hub.Clients.Group(payment.Order.UserId.ToString()).SendAsync("notificationReceived", new
            {
                type = "OrderConfirmed",
                message = $"Order {payment.Order.OrderNumber} payment confirmed!",
                timestamp = DateTime.UtcNow
            }, cancellationToken);

            // Admin notification: Order Paid
            await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
            {
                type = "OrderPaid",
                message = $"Order {payment.Order.OrderNumber} has been successfully paid.",
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    IHubContext<OrderHub> hub,
    IEnumerable<IPaymentProvider> providers,
    INotificationService notificationService
) : IPaymentService
{
    private IPaymentProvider GetProvider(string providerName)
    {
        var provider = providers.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            throw new InvalidOperationException($"Payment provider '{providerName}' is not supported.");
        }
        return provider;
    }

    public async Task<PaymentSessionResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        return await InitializePaymentAsync(request.OrderId, request.Provider, request.Currency, cancellationToken);
    }

    public async Task<PaymentSessionResponse> InitializePaymentAsync(Guid orderId, string providerName, string currency, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(x => x.Payment).FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");

        if (order.Payment is not null && order.Payment.Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Order already paid.");
        }

        var payment = order.Payment ?? new Payment
        {
            OrderId = order.Id,
            Provider = providerName,
            Amount = order.TotalAmount,
            Currency = currency.ToLowerInvariant(),
            Status = PaymentStatus.Pending
        };

        var provider = GetProvider(providerName);
        var response = await provider.InitializePaymentAsync(payment, cancellationToken);

        if (order.Payment is null) db.Payments.Add(payment);

        // Audit Log
        var audit = new AuditLog
        {
            Action = "PaymentCreated",
            EntityName = "Payment",
            EntityId = payment.Id.ToString(),
            UserId = order.UserId,
            Details = $"Payment initialized for Order {order.OrderNumber} via {providerName}. Amount: {payment.Amount} {payment.Currency}"
        };
        db.AuditLogs.Add(audit);

        await db.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<bool> VerifyPaymentAsync(Guid paymentId, string transactionId, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.Include(x => x.Order).ThenInclude(x => x.Items).FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment record not found.");

        var provider = GetProvider(payment.Provider);
        var isSuccess = await provider.VerifyPaymentAsync(payment, transactionId, cancellationToken);

        if (isSuccess)
        {
            // Create Transaction record
            var txn = new PaymentTransaction
            {
                PaymentId = payment.Id,
                GatewayTransactionId = transactionId,
                Amount = payment.Amount,
                Status = "Success"
            };
            db.PaymentTransactions.Add(txn);

            await MarkPaymentAsync(payment, PaymentStatus.Succeeded, cancellationToken);

            // Audit
            var audit = new AuditLog
            {
                Action = "PaymentVerified",
                EntityName = "Payment",
                EntityId = payment.Id.ToString(),
                UserId = payment.Order.UserId,
                Details = $"Payment verified successfully for Order {payment.Order.OrderNumber}. Gateway Txn: {transactionId}"
            };
            db.AuditLogs.Add(audit);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await MarkPaymentAsync(payment, PaymentStatus.Failed, cancellationToken);

            var audit = new AuditLog
            {
                Action = "PaymentFailed",
                EntityName = "Payment",
                EntityId = payment.Id.ToString(),
                UserId = payment.Order.UserId,
                Details = $"Payment verification failed for Order {payment.Order.OrderNumber}."
            };
            db.AuditLogs.Add(audit);
            await db.SaveChangesAsync(cancellationToken);
        }

        return isSuccess;
    }

    public async Task<bool> CompletePaymentAsync(CompletePaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.Include(x => x.Order).ThenInclude(x => x.Items).FirstOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken);
        if (payment is null) return false;
        await MarkPaymentAsync(payment, request.Status, cancellationToken);
        return true;
    }

    public async Task<bool> RefundPaymentAsync(Guid paymentId, decimal amount, string reason, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment record not found.");

        if (payment.Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Cannot refund an unpaid transaction.");
        }

        var provider = GetProvider(payment.Provider);
        var isSuccess = await provider.RefundPaymentAsync(payment, amount, reason, cancellationToken);

        if (isSuccess)
        {
            var refund = new ShopNest.Domain.Entities.Refund
            {
                PaymentId = payment.Id,
                TransactionId = $"ref_{Guid.NewGuid():N}",
                Amount = amount,
                Reason = reason,
                Status = "Completed"
            };
            db.Refunds.Add(refund);

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAtUtc = DateTime.UtcNow;

            var audit = new AuditLog
            {
                Action = "RefundCompleted",
                EntityName = "Payment",
                EntityId = payment.Id.ToString(),
                UserId = payment.Order.UserId,
                Details = $"Refund of {amount} processed for Order {payment.Order.OrderNumber}. Reason: {reason}"
            };
            db.AuditLogs.Add(audit);

            await db.SaveChangesAsync(cancellationToken);

            await notificationService.SendManualNotificationAsync(new SendManualNotificationRequest(
                payment.Order.UserId,
                "Refund Completed",
                $"A refund of ${amount:F2} has been successfully processed for Order {payment.Order.OrderNumber}. Reason: {reason}.",
                "Success",
                "Email",
                "Medium"
            ), cancellationToken);
        }

        return isSuccess;
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPaymentHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var payments = await db.Payments
            .Include(x => x.Order)
            .Where(x => x.Order.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentDto(
            p.Id,
            p.Provider,
            p.ProviderPaymentId,
            p.ProviderOrderId,
            p.Status,
            p.Amount,
            p.Currency
        )).ToList();
    }

    public async Task<PaymentDto?> GetPaymentDetailsAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);
        if (p == null) return null;

        return new PaymentDto(
            p.Id,
            p.Provider,
            p.ProviderPaymentId,
            p.ProviderOrderId,
            p.Status,
            p.Amount,
            p.Currency
        );
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

        if (status == PaymentStatus.Failed)
        {
            payment.Order.Status = OrderStatus.Cancelled;
        }

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

            // Templated notifications
            await notificationService.SendTemplatedNotificationAsync(
                payment.Order.UserId,
                "PaymentSuccess",
                new Dictionary<string, string>
                {
                    { "Amount", payment.Amount.ToString("F2") },
                    { "TransactionNumber", payment.ProviderPaymentId ?? payment.Id.ToString() },
                    { "PaymentMethod", payment.Provider }
                },
                "Payment",
                payment.Id.ToString(),
                cancellationToken);

            await notificationService.SendTemplatedNotificationAsync(
                payment.Order.UserId,
                "OrderConfirmation",
                new Dictionary<string, string>
                {
                    { "OrderNumber", payment.Order.OrderNumber },
                    { "TotalAmount", payment.Order.TotalAmount.ToString("F2") }
                },
                "Order",
                payment.Order.Id.ToString(),
                cancellationToken);
        }
        else if (status == PaymentStatus.Failed)
        {
            await notificationService.SendTemplatedNotificationAsync(
                payment.Order.UserId,
                "PaymentFailure",
                new Dictionary<string, string>
                {
                    { "Amount", payment.Amount.ToString("F2") },
                    { "TransactionNumber", payment.ProviderPaymentId ?? payment.Id.ToString() },
                    { "FailureReason", "Transaction refused by issuer gateway." }
                },
                "Payment",
                payment.Id.ToString(),
                cancellationToken);
        }
    }
}

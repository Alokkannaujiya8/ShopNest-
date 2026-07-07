using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services.Notifications;

public sealed class SMSNotificationProvider(ILogger<SMSNotificationProvider> logger) : INotificationProvider
{
    public string Channel => "SMS";

    public async Task<bool> SendAsync(string recipient, string title, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[SMS Channel Simulation] Target: {Recipient}, Title: {Title}, Message: {Message}", recipient, title, message);
        await Task.Delay(50, cancellationToken);
        return true;
    }
}

public sealed class PushNotificationProvider(ILogger<PushNotificationProvider> logger) : INotificationProvider
{
    public string Channel => "Push";

    public async Task<bool> SendAsync(string recipient, string title, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[Push Notification Simulation] Target: {Recipient}, Title: {Title}, Message: {Message}", recipient, title, message);
        await Task.Delay(50, cancellationToken);
        return true;
    }
}

public sealed class WhatsAppNotificationProvider(ILogger<WhatsAppNotificationProvider> logger) : INotificationProvider
{
    public string Channel => "WhatsApp";

    public async Task<bool> SendAsync(string recipient, string title, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[WhatsApp Channel Simulation] Target: {Recipient}, Title: {Title}, Message: {Message}", recipient, title, message);
        await Task.Delay(50, cancellationToken);
        return true;
    }
}

public sealed class TelegramNotificationProvider(ILogger<TelegramNotificationProvider> logger) : INotificationProvider
{
    public string Channel => "Telegram";

    public async Task<bool> SendAsync(string recipient, string title, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[Telegram Channel Simulation] Target: {Recipient}, Title: {Title}, Message: {Message}", recipient, title, message);
        await Task.Delay(50, cancellationToken);
        return true;
    }
}

using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services.Notifications;

public sealed class EmailNotificationProvider(IEmailService emailService) : INotificationProvider
{
    public string Channel => "Email";

    public async Task<bool> SendAsync(string recipient, string title, string message, CancellationToken cancellationToken)
    {
        await emailService.SendEmailAsync(recipient, title, message, cancellationToken);
        return true;
    }
}

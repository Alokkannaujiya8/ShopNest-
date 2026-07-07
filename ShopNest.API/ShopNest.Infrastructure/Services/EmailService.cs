using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string? _host;
    private readonly int _port;
    private readonly string? _username;
    private readonly string? _password;
    private readonly string _from;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _host = configuration["Smtp:Host"];
        var portStr = configuration["Smtp:Port"];
        _port = int.TryParse(portStr, out var p) ? p : 587;
        _username = configuration["Smtp:Username"];
        _password = configuration["Smtp:Password"];
        _from = configuration["Smtp:From"] ?? "noreply@shopnest.com";
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_host))
        {
            _logger.LogWarning("SMTP Host is not configured. Email to {To} with subject '{Subject}' simulated in logs:\n{Body}", to, subject, body);
            await Task.Delay(200, cancellationToken);
            return;
        }

        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = true
            };

            using var message = new MailMessage(_from, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Falling back to log simulation.", to);
            _logger.LogWarning("Fallback: Email simulation for {To} with subject '{Subject}':\n{Body}", to, subject, body);
        }
    }
}

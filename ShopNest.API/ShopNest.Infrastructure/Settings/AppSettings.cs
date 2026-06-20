namespace ShopNest.Infrastructure.Settings;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "ShopNest";
    public string Audience { get; set; } = "ShopNest.Client";
    public string Secret { get; set; } = "replace-this-development-secret-with-at-least-32-characters";
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 14;
}

public sealed class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}

public sealed class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "shopnest.orders";
}

public sealed class PaymentSettings
{
    public string StripeSecretKey { get; set; } = string.Empty;
    public string StripeWebhookSecret { get; set; } = string.Empty;
    public string RazorpayKeyId { get; set; } = string.Empty;
    public string RazorpayKeySecret { get; set; } = string.Empty;
}

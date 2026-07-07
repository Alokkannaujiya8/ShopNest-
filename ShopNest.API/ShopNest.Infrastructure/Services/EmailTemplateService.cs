using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services;

public sealed class EmailTemplateService : IEmailTemplateService
{
    private const string BaseHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>ShopNest Notification</title>
    <style>
        body { font-family: 'Inter', 'Helvetica Neue', Helvetica, Arial, sans-serif; background-color: #f8fafc; color: #1e293b; margin: 0; padding: 0; }
        .wrapper { max-width: 600px; margin: 40px auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1); border: 1px solid #e2e8f0; }
        .header { background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); padding: 32px; text-align: center; }
        .logo { font-size: 28px; font-weight: 800; color: #ffffff; letter-spacing: -0.05em; }
        .content { padding: 40px; line-height: 1.6; }
        .title { font-size: 22px; font-weight: 700; color: #0f172a; margin-top: 0; margin-bottom: 20px; }
        .button { display: inline-block; padding: 14px 28px; background-color: #4f46e5; color: #ffffff !important; font-weight: 600; text-decoration: none; border-radius: 8px; margin: 24px 0; text-align: center; }
        .footer { background-color: #f8fafc; padding: 24px; text-align: center; font-size: 13px; color: #64748b; border-top: 1px solid #e2e8f0; }
        .badge { display: inline-block; font-size: 32px; font-weight: 800; letter-spacing: 6px; padding: 16px 32px; background-color: #f1f5f9; border-radius: 10px; color: #4f46e5; margin: 20px 0; border: 1px dashed #cbd5e1; }
        .table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .table th { text-align: left; padding: 12px; border-bottom: 2px solid #e2e8f0; color: #475569; font-weight: 600; }
        .table td { padding: 12px; border-bottom: 1px solid #f1f5f9; color: #334155; }
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='header'>
            <div class='logo'>ShopNest</div>
        </div>
        <div class='content'>
            {{Content}}
        </div>
        <div class='footer'>
            &copy; 2026 ShopNest E-Commerce. All rights reserved.<br>
            If you did not request this email, please ignore it.
        </div>
    </div>
</body>
</html>";

    public string GetWelcomeEmailHtml(string userName)
    {
        var body = $@"
            <h1 class='title'>Welcome aboard, {userName}!</h1>
            <p>We are thrilled to welcome you to ShopNest, your new favorite destination for premium shopping.</p>
            <p>Explore thousands of products, curated categories, and exclusive discounts tailored just for you.</p>
            <a href='https://localhost:4200' class='button'>Start Shopping Now</a>
            <p>If you have any questions, feel free to reply to this email.</p>";

        return BaseHtml.Replace("{{Content}}", body);
    }

    public string GetOtpEmailHtml(string userName, string otpCode, int expiryMinutes = 5)
    {
        var body = $@"
            <h1 class='title'>Verify Your Email</h1>
            <p>Hi {userName},</p>
            <p>Please use the following One-Time Password (OTP) to complete your registration or verification request.</p>
            <div style='text-align: center;'>
                <div class='badge'>{otpCode}</div>
            </div>
            <p style='color: #ef4444; font-weight: 500;'>This OTP will expire in {expiryMinutes} minutes for your security. Please do not share this code with anyone.</p>";

        return BaseHtml.Replace("{{Content}}", body);
    }

    public string GetPasswordResetEmailHtml(string userName, string resetLink)
    {
        var body = $@"
            <h1 class='title'>Reset Your Password</h1>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password. Click the button below to choose a new password.</p>
            <a href='{resetLink}' class='button'>Reset Password</a>
            <p>If you did not request a password reset, you can safely ignore this email.</p>";

        return BaseHtml.Replace("{{Content}}", body);
    }

    public string GetInvoiceEmailHtml(string userName, string orderNumber, decimal totalAmount, string invoiceUrl)
    {
        var body = $@"
            <h1 class='title'>Your Invoice is Ready</h1>
            <p>Hi {userName},</p>
            <p>Thank you for your order! Your invoice for order <strong>#{orderNumber}</strong> has been generated.</p>
            <table class='table'>
                <tr>
                    <th>Description</th>
                    <th style='text-align: right;'>Amount</th>
                </tr>
                <tr>
                    <td>Order #{orderNumber} - Products Summary</td>
                    <td style='text-align: right;'>{totalAmount:C}</td>
                </tr>
                <tr>
                    <td style='font-weight: bold;'>Total Paid</td>
                    <td style='text-align: right; font-weight: bold; color: #4f46e5;'>{totalAmount:C}</td>
                </tr>
            </table>
            <a href='{invoiceUrl}' class='button'>Download PDF Invoice</a>";

        return BaseHtml.Replace("{{Content}}", body);
    }

    public string GetOrderConfirmationEmailHtml(string userName, string orderNumber, decimal totalAmount)
    {
        var body = $@"
            <h1 class='title'>Order Confirmed!</h1>
            <p>Hi {userName},</p>
            <p>We have received your order <strong>#{orderNumber}</strong> and we are preparing it for shipment.</p>
            <p>Your order total is <strong>{totalAmount:C}</strong>.</p>
            <p>We will send you another update with a tracking number once your package has been shipped.</p>
            <a href='https://localhost:4200/account/orders/{orderNumber}' class='button'>Track Your Order</a>";

        return BaseHtml.Replace("{{Content}}", body);
    }

    public string GetShippingUpdateEmailHtml(string userName, string orderNumber, string trackingNumber, string carrier)
    {
        var body = $@"
            <h1 class='title'>Your Order has Shipped!</h1>
            <p>Hi {userName},</p>
            <p>Great news! Your order <strong>#{orderNumber}</strong> has been shipped and is on its way.</p>
            <table class='table'>
                <tr>
                    <th>Courier Partner</th>
                    <th>Tracking Number</th>
                </tr>
                <tr>
                    <td>{carrier}</td>
                    <td><strong style='color: #4f46e5;'>{trackingNumber}</strong></td>
                </tr>
            </table>
            <a href='https://localhost:4200/account/orders/{orderNumber}' class='button'>View Delivery Status</a>";

        return BaseHtml.Replace("{{Content}}", body);
    }
}

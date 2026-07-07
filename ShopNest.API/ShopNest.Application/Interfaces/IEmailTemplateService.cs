using System.Threading.Tasks;

namespace ShopNest.Application.Interfaces;

public interface IEmailTemplateService
{
    string GetWelcomeEmailHtml(string userName);
    string GetOtpEmailHtml(string userName, string otpCode, int expiryMinutes = 5);
    string GetPasswordResetEmailHtml(string userName, string resetLink);
    string GetInvoiceEmailHtml(string userName, string orderNumber, decimal totalAmount, string invoiceUrl);
    string GetOrderConfirmationEmailHtml(string userName, string orderNumber, decimal totalAmount);
    string GetShippingUpdateEmailHtml(string userName, string orderNumber, string trackingNumber, string carrier);
}

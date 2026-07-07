using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ShopNest.API.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Content Security Policy (CSP)
        if (!headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: https://res.cloudinary.com; " +
                "connect-src 'self' https://api.stripe.com;");
        }

        // Prevent Clickjacking
        if (!headers.ContainsKey("X-Frame-Options"))
        {
            headers.Append("X-Frame-Options", "DENY");
        }

        // Prevent MIME Sniffing
        if (!headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Append("X-Content-Type-Options", "nosniff");
        }

        // Referrer Policy
        if (!headers.ContainsKey("Referrer-Policy"))
        {
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        }

        // Legacy XSS Protection
        if (!headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Append("X-XSS-Protection", "1; mode=block");
        }

        await _next(context);
    }
}

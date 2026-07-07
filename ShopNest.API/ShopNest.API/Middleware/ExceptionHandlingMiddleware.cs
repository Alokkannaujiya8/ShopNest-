using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShopNest.Application.Common;
using ShopNest.Application.Interfaces;

namespace ShopNest.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(x => x.ErrorMessage).ToList();
            await auditService.RecordErrorAsync(ex, context.Request.Path, context.Request.Method, null, "Warning");
            await WriteAsync(context, HttpStatusCode.BadRequest, "Validation failed.", errors);
        }
        catch (EmailNotVerifiedException ex)
        {
            await auditService.RecordErrorAsync(ex, context.Request.Path, context.Request.Method, null, "Warning");
            await WriteAsync(context, HttpStatusCode.BadRequest, "EmailNotVerified", [ex.Message]);
        }
        catch (UnauthorizedAccessException ex)
        {
            await auditService.RecordErrorAsync(ex, context.Request.Path, context.Request.Method, null, "Warning");
            await WriteAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await auditService.RecordErrorAsync(ex, context.Request.Path, context.Request.Method, null, "Warning");
            await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error");

            // Read request body safely if needed, but redact sensitive details
            string? body = null;
            try
            {
                if (context.Request.ContentLength > 0 && context.Request.ContentLength < 10240) // Less than 10KB
                {
                    context.Request.EnableBuffering();
                    context.Request.Body.Position = 0;
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    // Simple redactor for common sensitive parameters in request bodies
                    body = RedactSensitiveBody(body);
                }
            }
            catch { /* Fail silently */ }

            await auditService.RecordErrorAsync(ex, context.Request.Path, context.Request.Method, body, "Critical");
            await WriteAsync(context, HttpStatusCode.InternalServerError, "Unexpected server error.");
        }
    }

    private static string RedactSensitiveBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return body;

        var sensitiveKeys = new[] { "password", "token", "otp", "secret", "cvv", "cardNumber" };
        foreach (var key in sensitiveKeys)
        {
            // Simple regex replacement pattern for JSON strings
            var pattern = $"\"{key}\"\\s*:\\s*\"[^\"]+\"";
            body = System.Text.RegularExpressions.Regex.Replace(body, pattern, $"\"{key}\": \"[REDACTED]\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return body;
    }

    private static async Task WriteAsync(
        HttpContext context, 
        HttpStatusCode statusCode, 
        string message, 
        List<string>? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse.FailureResult(errors ?? [message], message);
        await context.Response.WriteAsJsonAsync(response);
    }
}

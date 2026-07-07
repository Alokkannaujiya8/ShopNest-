using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ShopNest.API.Middleware;

public sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();

        // Skip logging for health checks and log viewing APIs to prevent infinite logs
        if (path.Contains("/health") || path.Contains("/admin/logs"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogInformation("HTTP Request: {Method} {Path} [CorrelationId: {CorrelationId}]", 
            context.Request.Method, context.Request.Path, correlationId);

        await _next(context);

        stopwatch.Stop();
        var elapsed = stopwatch.ElapsedMilliseconds;

        if (elapsed > 500)
        {
            _logger.LogWarning("HTTP High-Latency: {Method} {Path} responded {StatusCode} in {Elapsed}ms [CorrelationId: {CorrelationId}]", 
                context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed, correlationId);
        }
        else
        {
            _logger.LogInformation("HTTP Response: {Method} {Path} responded {StatusCode} in {Elapsed}ms [CorrelationId: {CorrelationId}]", 
                context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed, correlationId);
        }
    }
}

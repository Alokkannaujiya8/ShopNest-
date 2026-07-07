using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ErrorLog : BaseEntity
{
    public string ExceptionMessage { get; set; } = string.Empty;
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? QueryString { get; set; }
    public string? RequestBody { get; set; } // Non-sensitive values only
    public string? UserId { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
    public string Severity { get; set; } = "Error"; // e.g. Warning, Error, Critical
}

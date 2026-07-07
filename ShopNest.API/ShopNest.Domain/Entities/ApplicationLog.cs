using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ApplicationLog : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "Information"; // Information, Warning, Error, Fatal
    public string? Exception { get; set; }
    public string? Properties { get; set; } // JSON properties
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
}

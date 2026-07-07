using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string? Role { get; set; }
    public string EventType { get; set; } = string.Empty; // Security, DataChange, System
    public string Module { get; set; } = string.Empty; // e.g. Catalog, Auth, Orders
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string Description { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? Browser { get; set; }
    public string? Device { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }

    // Compatibility property for legacy service logs
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Details 
    { 
        get => Description; 
        set => Description = value ?? string.Empty; 
    }
}

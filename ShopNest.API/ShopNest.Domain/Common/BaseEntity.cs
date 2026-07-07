namespace ShopNest.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Audit Fields
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft Delete Fields
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    
    // Concurrency Token
    public byte[] RowVersion { get; set; } = null!;
}

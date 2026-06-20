using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ProductImage : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

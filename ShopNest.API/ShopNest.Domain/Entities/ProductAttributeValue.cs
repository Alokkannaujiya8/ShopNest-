using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ProductAttributeValue : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid ProductAttributeId { get; set; }
    public ProductAttribute ProductAttribute { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}

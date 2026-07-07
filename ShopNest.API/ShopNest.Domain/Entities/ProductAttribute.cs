using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ProductAttribute : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Color, Size, Material, RAM, Storage, etc.
    public List<ProductAttributeValue> Values { get; set; } = [];
}

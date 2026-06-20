using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = [];
}

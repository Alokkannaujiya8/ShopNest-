using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public List<Product> Products { get; set; } = [];
}

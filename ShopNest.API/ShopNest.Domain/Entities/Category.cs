using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }
    public string? BannerUrl { get; set; }
    public string? BannerPublicId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsFeatured { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // SEO Fields
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // Self-referencing recursive fields
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];

    // Products link
    public List<Product> Products { get; set; } = [];
}

using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    
    // Unique Identification
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    // Pricing & Tax
    public decimal CostPrice { get; set; }
    public decimal Price { get; set; } // Selling Price
    public string? DiscountType { get; set; } // Flat, Percentage, None
    public decimal DiscountValue { get; set; }
    public DateTime? DiscountStartDate { get; set; }
    public DateTime? DiscountEndDate { get; set; }
    public decimal TaxPercentage { get; set; }

    // Inventory status
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public string StockStatus { get; set; } = "InStock"; // InStock, LowStock, OutOfStock

    // Shipping Properties
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }

    // SEO properties
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // Status Flags
    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public bool IsNewArrival { get; set; } = false;
    public bool IsBestSeller { get; set; } = false;

    // Hierarchy Connections
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public Guid? SubCategoryId { get; set; }
    public Category? SubCategory { get; set; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }

    // Collections
    public List<ProductImage> Images { get; set; } = [];
    public List<ProductVariant> Variants { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<Inventory> Inventories { get; set; } = [];
    public List<ProductCategory> ProductCategories { get; set; } = [];
    public List<ProductQuestion> Questions { get; set; } = [];
}

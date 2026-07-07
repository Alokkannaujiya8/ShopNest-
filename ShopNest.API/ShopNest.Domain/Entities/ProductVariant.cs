using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Name { get; set; } = string.Empty; // Color: Blue, Size: XL
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }

    public List<Inventory> Inventories { get; set; } = [];
    public List<ProductAttributeValue> AttributeValues { get; set; } = [];
}

using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ICollection<Inventory> Inventories { get; set; } = [];
}

using System;
using System.Collections.Generic;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public string Sku { get; set; } = string.Empty;

    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int CurrentStock { get; set; }
    public int AvailableStock { get; set; }
    public int ReservedStock { get; set; }

    public int MinimumStockLevel { get; set; }
    public int MaximumStockLevel { get; set; }
    public int ReorderLevel { get; set; }

    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal LastPurchasePrice { get; set; }

    public List<InventoryTransaction> Transactions { get; set; } = [];
}

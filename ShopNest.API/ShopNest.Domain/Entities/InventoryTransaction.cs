using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class InventoryTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; } = null!;
    
    public int Quantity { get; set; } // positive: add, negative: subtract
    public int PreviousStock { get; set; }
    public int UpdatedStock { get; set; }
    
    public string TransactionType { get; set; } = string.Empty; // StockIn, StockOut, Sale, Purchase, Return, CancelledOrder, Damaged, ManualAdjustment
    public string? Reason { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; } // OrderNumber, PO Number, etc
}

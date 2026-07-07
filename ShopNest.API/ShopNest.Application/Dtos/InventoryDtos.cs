using System;

namespace ShopNest.Application.Dtos;

public sealed record WarehouseDto(
    Guid Id,
    string Name,
    string Code,
    string? Address
);

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid? ProductVariantId,
    string? ProductVariantName,
    string Sku,
    Guid? WarehouseId,
    string? WarehouseName,
    int CurrentStock,
    int AvailableStock,
    int ReservedStock,
    int MinimumStockLevel,
    int MaximumStockLevel,
    int ReorderLevel,
    decimal UnitCost,
    decimal SellingPrice,
    decimal LastPurchasePrice,
    DateTime LastUpdated
);

public sealed record InventoryTransactionDto(
    Guid Id,
    string TransactionNumber,
    Guid InventoryId,
    string ProductName,
    string? VariantName,
    string Sku,
    int Quantity,
    int PreviousStock,
    int UpdatedStock,
    string TransactionType,
    string? Reason,
    string PerformedBy,
    string? ReferenceNumber,
    DateTime TransactionDate
);

public sealed record StockInRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    Guid? WarehouseId,
    int Quantity,
    decimal UnitCost,
    string PerformedBy,
    string? Reason,
    string? ReferenceNumber
);

public sealed record StockOutRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    Guid? WarehouseId,
    int Quantity,
    string PerformedBy,
    string? Reason,
    string? ReferenceNumber
);

public sealed record StockAdjustmentRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    Guid? WarehouseId,
    int NewQuantity,
    string PerformedBy,
    string Reason,
    string? ReferenceNumber
);

public sealed record UpdateInventoryLimitsRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    Guid? WarehouseId,
    int MinimumStockLevel,
    int MaximumStockLevel,
    int ReorderLevel
);

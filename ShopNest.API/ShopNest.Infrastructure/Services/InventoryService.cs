using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class InventoryService(
    ShopNestDbContext db,
    IHubContext<OrderHub> hub
) : IInventoryService
{
    public async Task<PagedResult<InventoryDto>> GetInventoryListAsync(
        string? query,
        Guid? productId,
        Guid? warehouseId,
        string? categoryId,
        string? stockStatus,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken)
    {
        var dbQuery = db.Inventories
            .Include(x => x.Product)
            .ThenInclude(x => x.Category)
            .Include(x => x.ProductVariant)
            .Include(x => x.Warehouse)
            .AsNoTracking();

        if (productId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            if (Guid.TryParse(categoryId, out var catGuid))
            {
                dbQuery = dbQuery.Where(x => x.Product.CategoryId == catGuid);
            }
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLower();
            dbQuery = dbQuery.Where(x =>
                x.Product.Name.ToLower().Contains(q) ||
                x.Sku.ToLower().Contains(q) ||
                (x.ProductVariant != null && x.ProductVariant.Name.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(stockStatus))
        {
            dbQuery = stockStatus.ToLower() switch
            {
                "outofstock" => dbQuery.Where(x => x.CurrentStock == 0),
                "lowstock" => dbQuery.Where(x => x.CurrentStock > 0 && x.CurrentStock <= x.MinimumStockLevel),
                "overstock" => dbQuery.Where(x => x.CurrentStock >= x.MaximumStockLevel),
                _ => dbQuery
            };
        }

        // Sorting
        dbQuery = sortBy?.ToLower() switch
        {
            "productname" => sortDescending
                ? dbQuery.OrderByDescending(x => x.Product.Name)
                : dbQuery.OrderBy(x => x.Product.Name),
            "currentstock" => sortDescending
                ? dbQuery.OrderByDescending(x => x.CurrentStock)
                : dbQuery.OrderBy(x => x.CurrentStock),
            "availablestock" => sortDescending
                ? dbQuery.OrderByDescending(x => x.AvailableStock)
                : dbQuery.OrderBy(x => x.AvailableStock),
            _ => sortDescending
                ? dbQuery.OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
                : dbQuery.OrderBy(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
        };

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryDto>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<InventoryTransactionDto>> GetInventoryTransactionsAsync(
        string? query,
        string? transactionType,
        Guid? productId,
        Guid? warehouseId,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken)
    {
        var dbQuery = db.InventoryTransactions
            .Include(x => x.Inventory)
            .ThenInclude(x => x.Product)
            .Include(x => x.Inventory.ProductVariant)
            .AsNoTracking();

        if (productId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Inventory.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Inventory.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            dbQuery = dbQuery.Where(x => x.TransactionType == transactionType);
        }

        if (startDate.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAtUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAtUtc <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLower();
            dbQuery = dbQuery.Where(x =>
                x.TransactionNumber.ToLower().Contains(q) ||
                x.Inventory.Product.Name.ToLower().Contains(q) ||
                x.Inventory.Sku.ToLower().Contains(q) ||
                (x.Reason != null && x.Reason.ToLower().Contains(q)));
        }

        dbQuery = sortDescending
            ? dbQuery.OrderByDescending(x => x.CreatedAtUtc)
            : dbQuery.OrderBy(x => x.CreatedAtUtc);

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapToTransactionDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryTransactionDto>(items, page, pageSize, totalCount);
    }

    public async Task<InventoryDto> GetInventoryByProductAsync(Guid productId, Guid? productVariantId, Guid? warehouseId, CancellationToken cancellationToken)
    {
        var whId = warehouseId ?? await GetDefaultWarehouseIdAsync(cancellationToken);
        var inventory = await db.Inventories
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductVariantId == productVariantId && x.WarehouseId == whId, cancellationToken);

        if (inventory == null)
        {
            var product = await db.Products.FindAsync([productId], cancellationToken) 
                ?? throw new InvalidOperationException("Product not found.");
            
            var sku = product.Sku;
            if (productVariantId.HasValue)
            {
                var variant = await db.ProductVariants.FindAsync([productVariantId.Value], cancellationToken);
                if (variant != null) sku = variant.Sku;
            }

            inventory = new Inventory
            {
                ProductId = productId,
                ProductVariantId = productVariantId,
                WarehouseId = whId,
                Sku = sku,
                CurrentStock = 0,
                AvailableStock = 0,
                ReservedStock = 0,
                MinimumStockLevel = 5,
                MaximumStockLevel = 100,
                ReorderLevel = 10,
                UnitCost = product.CostPrice > 0 ? product.CostPrice : product.Price * 0.6m,
                SellingPrice = product.Price
            };

            db.Inventories.Add(inventory);
            await db.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(inventory);
    }

    public async Task<InventoryDto> StockInAsync(StockInRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be positive.");

        var whId = request.WarehouseId ?? await GetDefaultWarehouseIdAsync(cancellationToken);
        var inventory = await GetOrCreateInventoryEntityAsync(request.ProductId, request.ProductVariantId, whId, cancellationToken);

        var prevStock = inventory.CurrentStock;
        inventory.CurrentStock += request.Quantity;
        inventory.AvailableStock = inventory.CurrentStock - inventory.ReservedStock;
        inventory.LastPurchasePrice = request.UnitCost;
        if (request.UnitCost > 0)
        {
            inventory.UnitCost = request.UnitCost;
        }
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        var tx = new InventoryTransaction
        {
            TransactionNumber = GenerateTransactionNumber(),
            InventoryId = inventory.Id,
            Quantity = request.Quantity,
            PreviousStock = prevStock,
            UpdatedStock = inventory.CurrentStock,
            TransactionType = "StockIn",
            Reason = request.Reason ?? "Stock In Purchase / Restock",
            PerformedBy = request.PerformedBy,
            ReferenceNumber = request.ReferenceNumber
        };

        db.InventoryTransactions.Add(tx);
        await db.SaveChangesAsync(cancellationToken);

        await UpdateProductStockCachesAsync(inventory.ProductId, inventory.ProductVariantId, cancellationToken);
        await CheckAlertsAndNotifyAsync(inventory, cancellationToken);

        return MapToDto(inventory);
    }

    public async Task<InventoryDto> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be positive.");

        var whId = request.WarehouseId ?? await GetDefaultWarehouseIdAsync(cancellationToken);
        var inventory = await GetOrCreateInventoryEntityAsync(request.ProductId, request.ProductVariantId, whId, cancellationToken);

        if (inventory.AvailableStock < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient available stock. Current stock is {inventory.AvailableStock}.");
        }

        var prevStock = inventory.CurrentStock;
        inventory.CurrentStock -= request.Quantity;
        inventory.AvailableStock = inventory.CurrentStock - inventory.ReservedStock;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        var tx = new InventoryTransaction
        {
            TransactionNumber = GenerateTransactionNumber(),
            InventoryId = inventory.Id,
            Quantity = -request.Quantity,
            PreviousStock = prevStock,
            UpdatedStock = inventory.CurrentStock,
            TransactionType = "StockOut",
            Reason = request.Reason ?? "Stock Out / Sales Deduction",
            PerformedBy = request.PerformedBy,
            ReferenceNumber = request.ReferenceNumber
        };

        db.InventoryTransactions.Add(tx);
        await db.SaveChangesAsync(cancellationToken);

        await UpdateProductStockCachesAsync(inventory.ProductId, inventory.ProductVariantId, cancellationToken);
        await CheckAlertsAndNotifyAsync(inventory, cancellationToken);

        return MapToDto(inventory);
    }

    public async Task<InventoryDto> StockAdjustmentAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        if (request.NewQuantity < 0) throw new ArgumentException("New stock quantity cannot be negative.");

        var whId = request.WarehouseId ?? await GetDefaultWarehouseIdAsync(cancellationToken);
        var inventory = await GetOrCreateInventoryEntityAsync(request.ProductId, request.ProductVariantId, whId, cancellationToken);

        var prevStock = inventory.CurrentStock;
        var diff = request.NewQuantity - prevStock;
        if (diff == 0) return MapToDto(inventory);

        inventory.CurrentStock = request.NewQuantity;
        inventory.AvailableStock = inventory.CurrentStock - inventory.ReservedStock;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        var tx = new InventoryTransaction
        {
            TransactionNumber = GenerateTransactionNumber(),
            InventoryId = inventory.Id,
            Quantity = diff,
            PreviousStock = prevStock,
            UpdatedStock = inventory.CurrentStock,
            TransactionType = "ManualAdjustment",
            Reason = request.Reason ?? "Manual Stock Correction",
            PerformedBy = request.PerformedBy,
            ReferenceNumber = request.ReferenceNumber
        };

        db.InventoryTransactions.Add(tx);
        await db.SaveChangesAsync(cancellationToken);

        await UpdateProductStockCachesAsync(inventory.ProductId, inventory.ProductVariantId, cancellationToken);
        await CheckAlertsAndNotifyAsync(inventory, cancellationToken);

        return MapToDto(inventory);
    }

    public async Task<InventoryDto> UpdateInventoryLimitsAsync(UpdateInventoryLimitsRequest request, CancellationToken cancellationToken)
    {
        var whId = request.WarehouseId ?? await GetDefaultWarehouseIdAsync(cancellationToken);
        var inventory = await GetOrCreateInventoryEntityAsync(request.ProductId, request.ProductVariantId, whId, cancellationToken);

        inventory.MinimumStockLevel = request.MinimumStockLevel;
        inventory.MaximumStockLevel = request.MaximumStockLevel;
        inventory.ReorderLevel = request.ReorderLevel;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return MapToDto(inventory);
    }

    public async Task<List<WarehouseDto>> GetWarehousesAsync(CancellationToken cancellationToken)
    {
        return await db.Warehouses
            .AsNoTracking()
            .Select(x => new WarehouseDto(x.Id, x.Name, x.Code, x.Address))
            .ToListAsync(cancellationToken);
    }

    // Helpers
    private async Task<Guid> GetDefaultWarehouseIdAsync(CancellationToken cancellationToken)
    {
        var wh = await db.Warehouses.FirstOrDefaultAsync(cancellationToken);
        if (wh == null)
        {
            wh = new Warehouse { Name = "Main Warehouse", Code = "WH-MAIN", Address = "Default HQ Location" };
            db.Warehouses.Add(wh);
            await db.SaveChangesAsync(cancellationToken);
        }
        return wh.Id;
    }

    private async Task<Inventory> GetOrCreateInventoryEntityAsync(Guid productId, Guid? productVariantId, Guid warehouseId, CancellationToken cancellationToken)
    {
        var inventory = await db.Inventories
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductVariantId == productVariantId && x.WarehouseId == warehouseId, cancellationToken);

        if (inventory == null)
        {
            var product = await db.Products.FindAsync([productId], cancellationToken) 
                ?? throw new InvalidOperationException("Product not found.");
            
            var sku = product.Sku;
            if (productVariantId.HasValue)
            {
                var variant = await db.ProductVariants.FindAsync([productVariantId.Value], cancellationToken);
                if (variant != null) sku = variant.Sku;
            }

            inventory = new Inventory
            {
                ProductId = productId,
                ProductVariantId = productVariantId,
                WarehouseId = warehouseId,
                Sku = sku,
                CurrentStock = 0,
                AvailableStock = 0,
                ReservedStock = 0,
                MinimumStockLevel = 5,
                MaximumStockLevel = 100,
                ReorderLevel = 10,
                UnitCost = product.CostPrice > 0 ? product.CostPrice : product.Price * 0.6m,
                SellingPrice = product.Price
            };

            db.Inventories.Add(inventory);
            await db.SaveChangesAsync(cancellationToken);
        }

        return inventory;
    }

    private string GenerateTransactionNumber()
    {
        return $"TX-INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }

    private async Task UpdateProductStockCachesAsync(Guid productId, Guid? productVariantId, CancellationToken cancellationToken)
    {
        // Sum up total stock for this specific variant across all warehouses
        if (productVariantId.HasValue)
        {
            var variant = await db.ProductVariants.FindAsync([productVariantId.Value], cancellationToken);
            if (variant != null)
            {
                var varStock = await db.Inventories
                    .Where(x => x.ProductVariantId == productVariantId.Value)
                    .SumAsync(x => x.CurrentStock, cancellationToken);
                
                variant.StockQuantity = varStock;
                variant.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        // Sum up total stock for base product across all warehouses and all variants
        var product = await db.Products.FindAsync([productId], cancellationToken);
        if (product != null)
        {
            var totalStock = await db.Inventories
                .Where(x => x.ProductId == productId)
                .SumAsync(x => x.CurrentStock, cancellationToken);

            product.StockQuantity = totalStock;
            product.StockStatus = totalStock == 0 ? "OutOfStock" : (totalStock <= product.MinimumStock ? "LowStock" : "InStock");
            product.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckAlertsAndNotifyAsync(Inventory inventory, CancellationToken cancellationToken)
    {
        string? alertType = null;
        string? message = null;

        if (inventory.CurrentStock == 0)
        {
            alertType = "ProductOutOfStock";
            message = $"{inventory.Product.Name} is completely OUT OF STOCK!";
        }
        else if (inventory.CurrentStock <= inventory.MinimumStockLevel)
        {
            alertType = "ProductStockLow";
            message = $"{inventory.Product.Name} is running low on stock ({inventory.CurrentStock} remaining).";
        }
        else if (inventory.CurrentStock >= inventory.MaximumStockLevel)
        {
            alertType = "ProductOverstock";
            message = $"{inventory.Product.Name} has overstock ({inventory.CurrentStock} items on hand).";
        }

        if (alertType != null && message != null)
        {
            await hub.Clients.Group("Admin").SendAsync("notificationReceived", new
            {
                type = alertType,
                message = message,
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
    }

    private static InventoryDto MapToDto(Inventory x) => new(
        x.Id,
        x.ProductId,
        x.Product?.Name ?? string.Empty,
        x.Product?.Sku ?? string.Empty,
        x.ProductVariantId,
        x.ProductVariant?.Name,
        x.Sku,
        x.WarehouseId,
        x.Warehouse?.Name,
        x.CurrentStock,
        x.AvailableStock,
        x.ReservedStock,
        x.MinimumStockLevel,
        x.MaximumStockLevel,
        x.ReorderLevel,
        x.UnitCost,
        x.SellingPrice,
        x.LastPurchasePrice,
        x.UpdatedAtUtc ?? x.CreatedAtUtc
    );

    private static InventoryTransactionDto MapToTransactionDto(InventoryTransaction x) => new(
        x.Id,
        x.TransactionNumber,
        x.InventoryId,
        x.Inventory?.Product?.Name ?? string.Empty,
        x.Inventory?.ProductVariant?.Name,
        x.Inventory?.Sku ?? string.Empty,
        x.Quantity,
        x.PreviousStock,
        x.UpdatedStock,
        x.TransactionType,
        x.Reason,
        x.PerformedBy,
        x.ReferenceNumber,
        x.CreatedAtUtc
    );
}

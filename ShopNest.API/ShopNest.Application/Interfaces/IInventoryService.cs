using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryDto>> GetInventoryListAsync(
        string? query,
        Guid? productId,
        Guid? warehouseId,
        string? categoryId,
        string? stockStatus, // LowStock, OutOfStock, Overstock
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken);

    Task<PagedResult<InventoryTransactionDto>> GetInventoryTransactionsAsync(
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
        CancellationToken cancellationToken);

    Task<InventoryDto> GetInventoryByProductAsync(Guid productId, Guid? productVariantId, Guid? warehouseId, CancellationToken cancellationToken);

    Task<InventoryDto> StockInAsync(StockInRequest request, CancellationToken cancellationToken);
    Task<InventoryDto> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken);
    Task<InventoryDto> StockAdjustmentAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
    Task<InventoryDto> UpdateInventoryLimitsAsync(UpdateInventoryLimitsRequest request, CancellationToken cancellationToken);
    Task<List<WarehouseDto>> GetWarehousesAsync(CancellationToken cancellationToken);
}

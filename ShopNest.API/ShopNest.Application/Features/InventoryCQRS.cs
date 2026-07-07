using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

// Commands
public sealed record StockInCommand(StockInRequest Request) : IRequest<InventoryDto>;
public sealed record StockOutCommand(StockOutRequest Request) : IRequest<InventoryDto>;
public sealed record AdjustStockCommand(StockAdjustmentRequest Request) : IRequest<InventoryDto>;
public sealed record UpdateInventoryLimitsCommand(UpdateInventoryLimitsRequest Request) : IRequest<InventoryDto>;

// Queries
public sealed record GetInventoryListQuery(
    string? Query,
    Guid? ProductId,
    Guid? WarehouseId,
    string? CategoryId,
    string? StockStatus,
    int Page,
    int PageSize,
    string? SortBy,
    bool SortDescending
) : IRequest<PagedResult<InventoryDto>>;

public sealed record GetInventoryTransactionsQuery(
    string? Query,
    string? TransactionType,
    Guid? ProductId,
    Guid? WarehouseId,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page,
    int PageSize,
    string? SortBy,
    bool SortDescending
) : IRequest<PagedResult<InventoryTransactionDto>>;

public sealed record GetWarehousesQuery : IRequest<List<WarehouseDto>>;

// Handlers
public sealed class InventoryCQRSHandlers(IInventoryService service) :
    IRequestHandler<StockInCommand, InventoryDto>,
    IRequestHandler<StockOutCommand, InventoryDto>,
    IRequestHandler<AdjustStockCommand, InventoryDto>,
    IRequestHandler<UpdateInventoryLimitsCommand, InventoryDto>,
    IRequestHandler<GetInventoryListQuery, PagedResult<InventoryDto>>,
    IRequestHandler<GetInventoryTransactionsQuery, PagedResult<InventoryTransactionDto>>,
    IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
{
    public Task<InventoryDto> Handle(StockInCommand request, CancellationToken cancellationToken) =>
        service.StockInAsync(request.Request, cancellationToken);

    public Task<InventoryDto> Handle(StockOutCommand request, CancellationToken cancellationToken) =>
        service.StockOutAsync(request.Request, cancellationToken);

    public Task<InventoryDto> Handle(AdjustStockCommand request, CancellationToken cancellationToken) =>
        service.StockAdjustmentAsync(request.Request, cancellationToken);

    public Task<InventoryDto> Handle(UpdateInventoryLimitsCommand request, CancellationToken cancellationToken) =>
        service.UpdateInventoryLimitsAsync(request.Request, cancellationToken);

    public Task<PagedResult<InventoryDto>> Handle(GetInventoryListQuery request, CancellationToken cancellationToken) =>
        service.GetInventoryListAsync(
            request.Query,
            request.ProductId,
            request.WarehouseId,
            request.CategoryId,
            request.StockStatus,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

    public Task<PagedResult<InventoryTransactionDto>> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken) =>
        service.GetInventoryTransactionsAsync(
            request.Query,
            request.TransactionType,
            request.ProductId,
            request.WarehouseId,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

    public Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken) =>
        service.GetWarehousesAsync(cancellationToken);
}

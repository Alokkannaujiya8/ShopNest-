using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public sealed class InventoryController(IMediator mediator) : ControllerBase
{
    private string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? "System Admin";

    [HttpGet]
    public async Task<ActionResult<PagedResult<InventoryDto>>> GetInventoryList(
        [FromQuery] string? query,
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] string? categoryId,
        [FromQuery] string? stockStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInventoryListQuery(
            query, productId, warehouseId, categoryId, stockStatus, page, pageSize, sortBy, sortDescending
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<InventoryDto>> GetInventoryByProduct(
        Guid productId,
        [FromQuery] Guid? productVariantId,
        [FromQuery] Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        var query = new GetInventoryListQuery(
            null, productId, warehouseId, null, null, 1, 1, null, false
        );
        var result = await mediator.Send(query, cancellationToken);
        if (result.Items.Count == 0)
        {
            // Trigger auto-creation if not found
            var serviceQuery = await mediator.Send(new StockInCommand(new StockInRequest(
                productId, productVariantId, warehouseId, 0, 0, CurrentUserEmail, "Initialize", null
            )), cancellationToken);
            return Ok(serviceQuery);
        }
        return Ok(result.Items[0]);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<PagedResult<InventoryTransactionDto>>> GetInventoryTransactions(
        [FromQuery] string? query,
        [FromQuery] string? transactionType,
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInventoryTransactionsQuery(
            query, transactionType, productId, warehouseId, startDate, endDate, page, pageSize, sortBy, sortDescending
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<PagedResult<InventoryDto>>> GetLowStockProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInventoryListQuery(
            null, null, null, null, "LowStock", page, pageSize, "currentstock", false
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("out-of-stock")]
    public async Task<ActionResult<PagedResult<InventoryDto>>> GetOutOfStockProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInventoryListQuery(
            null, null, null, null, "OutOfStock", page, pageSize, "currentstock", false
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("stock-in")]
    public async Task<ActionResult<InventoryDto>> StockIn(
        [FromBody] StockInRequest request,
        CancellationToken cancellationToken)
    {
        // Enforce PerformedBy to current logged-in user
        var req = request with { PerformedBy = CurrentUserEmail };
        var result = await mediator.Send(new StockInCommand(req), cancellationToken);
        return Ok(result);
    }

    [HttpPost("stock-out")]
    public async Task<ActionResult<InventoryDto>> StockOut(
        [FromBody] StockOutRequest request,
        CancellationToken cancellationToken)
    {
        var req = request with { PerformedBy = CurrentUserEmail };
        var result = await mediator.Send(new StockOutCommand(req), cancellationToken);
        return Ok(result);
    }

    [HttpPost("adjust")]
    public async Task<ActionResult<InventoryDto>> StockAdjustment(
        [FromBody] StockAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        var req = request with { PerformedBy = CurrentUserEmail };
        var result = await mediator.Send(new AdjustStockCommand(req), cancellationToken);
        return Ok(result);
    }

    [HttpPut("limits")]
    public async Task<ActionResult<InventoryDto>> UpdateInventoryLimits(
        [FromBody] UpdateInventoryLimitsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateInventoryLimitsCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<List<WarehouseDto>>> GetWarehouses(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWarehousesQuery(), cancellationToken);
        return Ok(result);
    }
}

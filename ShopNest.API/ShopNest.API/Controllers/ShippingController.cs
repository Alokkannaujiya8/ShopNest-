using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/shipping")]
public sealed class ShippingController(IShippingService shippingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShipmentDto>>> GetShipments(
        [FromQuery] string? courier,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole("Admin");
        Guid? userId = isAdmin ? null : User.UserId();
        var result = await shippingService.GetShipmentsAsync(userId, courier, status, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentDto>> GetShipmentById(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var shipment = await shippingService.GetShipmentByIdAsync(id, isAdmin ? null : User.UserId(), cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpGet("tracking/{trackingNumber}")]
    public async Task<ActionResult<ShipmentDto>> GetShipmentByTrackingNumber(string trackingNumber, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var shipment = await shippingService.GetShipmentByTrackingNumberAsync(trackingNumber, isAdmin ? null : User.UserId(), cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpGet("{id:guid}/timeline")]
    public async Task<ActionResult<List<ShipmentTrackingDto>>> GetTimeline(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var timeline = await shippingService.GetShipmentTimelineAsync(id, isAdmin ? null : User.UserId(), cancellationToken);
        return Ok(timeline);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ShipmentDto>> CreateShipment([FromBody] CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = await shippingService.CreateShipmentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetShipmentById), new { id = shipment.Id }, shipment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/assign")]
    public async Task<ActionResult<ShipmentDto>> AssignCourier(Guid id, [FromBody] ShopNest.Application.Dtos.AssignCourierRequest request, CancellationToken cancellationToken)
    {
        var shipment = await shippingService.AssignCourierAsync(id, request, cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ShipmentDto>> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
    {
        var shipment = await shippingService.UpdateShipmentStatusAsync(id, request, cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/reschedule")]
    public async Task<ActionResult<ShipmentDto>> RescheduleDelivery(Guid id, [FromBody] RescheduleDeliveryRequest request, CancellationToken cancellationToken)
    {
        var shipment = await shippingService.RescheduleDeliveryAsync(id, request, cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/deliver")]
    public async Task<ActionResult<ShipmentDto>> MarkDelivered(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await shippingService.MarkDeliveredAsync(id, cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/fail")]
    public async Task<ActionResult<ShipmentDto>> MarkFailed(Guid id, [FromBody] FailDeliveryRequest request, CancellationToken cancellationToken)
    {
        var shipment = await shippingService.MarkFailedAsync(id, request.Reason, cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpGet("couriers")]
    public async Task<ActionResult<List<CourierDto>>> GetCouriers(CancellationToken cancellationToken)
    {
        var couriers = await shippingService.GetCouriersAsync(cancellationToken);
        return Ok(couriers);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("couriers")]
    public async Task<ActionResult<CourierDto>> CreateCourier([FromBody] CreateCourierRequest request, CancellationToken cancellationToken)
    {
        var courier = await shippingService.CreateCourierAsync(request, cancellationToken);
        return Ok(courier);
    }
}

public sealed record FailDeliveryRequest(string Reason);

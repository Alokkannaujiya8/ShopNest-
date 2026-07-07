using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Orders;

namespace ShopNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    public Task<PagedResult<OrderDto>> MyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        mediator.Send(new GetMyOrdersQuery(User.UserId(), page, pageSize), cancellationToken);

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<PagedResult<OrderDto>> All([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        mediator.Send(new GetAllOrdersQuery(page, pageSize), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var order = await mediator.Send(new GetOrderByIdQuery(id, isAdmin ? null : User.UserId()), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetByNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var order = await mediator.Send(new GetOrderByNumberQuery(orderNumber, isAdmin ? null : User.UserId()), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var order = await mediator.Send(new CancelOrderCommand(id, isAdmin ? null : User.UserId(), request.Reason), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new UpdateOrderStatusCommand(id, request), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/payment-status")]
    public async Task<ActionResult<OrderDto>> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new UpdatePaymentStatusCommand(id, request.PaymentStatus), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<OrderDto>> RequestReturn(Guid id, [FromBody] ReturnRequestModel request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new RequestReturnCommand(id, User.UserId(), request.Reason), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<OrderDto>> RequestRefund(Guid id, [FromBody] RefundRequestModel request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new RequestRefundCommand(id, User.UserId(), request.Reason), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/courier")]
    public async Task<ActionResult<OrderDto>> AssignCourier(Guid id, [FromBody] AssignCourierRequest request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new AssignCourierCommand(id, request.CourierPartner, request.TrackingNumber), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [HttpGet("{id:guid}/timeline")]
    public async Task<ActionResult<List<OrderStatusHistoryDto>>> GetTimeline(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        return await mediator.Send(new GetOrderTimelineQuery(id, isAdmin ? null : User.UserId()), cancellationToken);
    }

    [HttpGet("{id:guid}/track")]
    public async Task<ActionResult<List<OrderTrackingDto>>> GetTracking(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        return await mediator.Send(new GetOrderTrackingQuery(id, isAdmin ? null : User.UserId()), cancellationToken);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/tracking-update")]
    public async Task<ActionResult<OrderDto>> AddTrackingUpdate(Guid id, [FromBody] TrackingUpdateRequestModel request, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new AddTrackingUpdateCommand(id, request.Status, request.Location), cancellationToken);
        return order is null ? NotFound() : order;
    }

    [HttpGet("{id:guid}/invoice")]
    public async Task<IActionResult> DownloadInvoice(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var order = await mediator.Send(new GetOrderByIdQuery(id, isAdmin ? null : User.UserId()), cancellationToken);
        if (order is null) return NotFound();

        var invoiceText = $"SHOPNEST INVOICE\n" +
                          $"===========================\n" +
                          $"Order Number: {order.OrderNumber}\n" +
                          $"Date: {DateTime.UtcNow:yyyy-MM-dd}\n" +
                          $"Status: {order.Status}\n" +
                          $"Shipping Address:\n{order.ShippingAddress}\n\n" +
                          $"Billing Address:\n{order.BillingAddress ?? order.ShippingAddress}\n" +
                          $"===========================\n" +
                          $"Items:\n";

        foreach (var item in order.Items)
        {
            invoiceText += $"- {item.ProductName} x {item.Quantity} | SKU: {item.Sku} | Price: {item.UnitPrice:C} | Total: {item.LineTotal:C}\n";
        }

        invoiceText += $"===========================\n" +
                       $"Shipping Cost: {order.ShippingCost:C}\n" +
                       $"Tax: {order.Tax:C}\n" +
                       $"Discount: {order.Discount:C}\n" +
                       $"Grand Total: {order.TotalAmount:C}\n" +
                       $"===========================\n" +
                       $"Thank you for shopping with ShopNest!\n";

        var bytes = System.Text.Encoding.UTF8.GetBytes(invoiceText);
        return File(bytes, "text/plain", $"Invoice_{order.OrderNumber}.txt");
    }
}

public sealed record CancelOrderRequest(string Reason);
public sealed record UpdatePaymentStatusRequest(string PaymentStatus);
public sealed record ReturnRequestModel(string Reason);
public sealed record RefundRequestModel(string Reason);
public sealed record AssignCourierRequest(string CourierPartner, string TrackingNumber);
public sealed record TrackingUpdateRequestModel(string Status, string Location);

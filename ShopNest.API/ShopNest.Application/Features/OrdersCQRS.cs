using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Orders;

public sealed record CheckoutCommand(Guid UserId, CheckoutRequest Request) : IRequest<OrderDto>;
public sealed record GetMyOrdersQuery(Guid UserId, int Page, int PageSize) : IRequest<PagedResult<OrderDto>>;
public sealed record GetAllOrdersQuery(int Page, int PageSize) : IRequest<PagedResult<OrderDto>>;
public sealed record GetOrderByIdQuery(Guid OrderId, Guid? UserId) : IRequest<OrderDto?>;
public sealed record UpdateOrderStatusCommand(Guid OrderId, UpdateOrderStatusRequest Request) : IRequest<OrderDto?>;
public sealed record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;
public sealed record UpdateInventoryCommand(Guid ProductId, InventoryUpdateRequest Request) : IRequest<bool>;

// Extended CQRS Records for Order Management
public sealed record GetOrderByNumberQuery(string OrderNumber, Guid? UserId) : IRequest<OrderDto?>;
public sealed record CancelOrderCommand(Guid OrderId, Guid? UserId, string Reason) : IRequest<OrderDto?>;
public sealed record UpdatePaymentStatusCommand(Guid OrderId, string PaymentStatus) : IRequest<OrderDto?>;
public sealed record RequestReturnCommand(Guid OrderId, Guid UserId, string Reason) : IRequest<OrderDto?>;
public sealed record RequestRefundCommand(Guid OrderId, Guid UserId, string Reason) : IRequest<OrderDto?>;
public sealed record AssignCourierCommand(Guid OrderId, string CourierPartner, string TrackingNumber) : IRequest<OrderDto?>;
public sealed record GetOrderTimelineQuery(Guid OrderId, Guid? UserId) : IRequest<List<OrderStatusHistoryDto>>;
public sealed record GetOrderTrackingQuery(Guid OrderId, Guid? UserId) : IRequest<List<OrderTrackingDto>>;
public sealed record AddTrackingUpdateCommand(Guid OrderId, string Status, string Location) : IRequest<OrderDto?>;

public sealed class OrderHandlers(ICartOrderService orders, IAdminService admin) :
    IRequestHandler<CheckoutCommand, OrderDto>,
    IRequestHandler<GetMyOrdersQuery, PagedResult<OrderDto>>,
    IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>,
    IRequestHandler<GetOrderByIdQuery, OrderDto?>,
    IRequestHandler<UpdateOrderStatusCommand, OrderDto?>,
    IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>,
    IRequestHandler<UpdateInventoryCommand, bool>,
    IRequestHandler<GetOrderByNumberQuery, OrderDto?>,
    IRequestHandler<CancelOrderCommand, OrderDto?>,
    IRequestHandler<UpdatePaymentStatusCommand, OrderDto?>,
    IRequestHandler<RequestReturnCommand, OrderDto?>,
    IRequestHandler<RequestRefundCommand, OrderDto?>,
    IRequestHandler<AssignCourierCommand, OrderDto?>,
    IRequestHandler<GetOrderTimelineQuery, List<OrderStatusHistoryDto>>,
    IRequestHandler<GetOrderTrackingQuery, List<OrderTrackingDto>>,
    IRequestHandler<AddTrackingUpdateCommand, OrderDto?>
{
    public Task<OrderDto> Handle(CheckoutCommand command, CancellationToken cancellationToken) =>
        orders.CheckoutAsync(command.UserId, command.Request, cancellationToken);

    public Task<PagedResult<OrderDto>> Handle(GetMyOrdersQuery query, CancellationToken cancellationToken) =>
        orders.GetOrdersAsync(query.UserId, query.Page, query.PageSize, cancellationToken);

    public Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken) =>
        orders.GetOrdersAsync(null, query.Page, query.PageSize, cancellationToken);

    public Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken) =>
        orders.GetOrderAsync(query.OrderId, query.UserId, cancellationToken);

    public Task<OrderDto?> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken) =>
        orders.UpdateOrderStatusAsync(command.OrderId, command.Request, cancellationToken);

    public Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken) =>
        admin.GetDashboardStatsAsync(cancellationToken);

    public Task<bool> Handle(UpdateInventoryCommand command, CancellationToken cancellationToken) =>
        admin.UpdateInventoryAsync(command.ProductId, command.Request, cancellationToken);

    public Task<OrderDto?> Handle(GetOrderByNumberQuery query, CancellationToken cancellationToken) =>
        orders.GetOrderByNumberAsync(query.OrderNumber, query.UserId, cancellationToken);

    public Task<OrderDto?> Handle(CancelOrderCommand command, CancellationToken cancellationToken) =>
        orders.CancelOrderAsync(command.OrderId, command.UserId, command.Reason, cancellationToken);

    public Task<OrderDto?> Handle(UpdatePaymentStatusCommand command, CancellationToken cancellationToken) =>
        orders.UpdatePaymentStatusAsync(command.OrderId, command.PaymentStatus, cancellationToken);

    public Task<OrderDto?> Handle(RequestReturnCommand command, CancellationToken cancellationToken) =>
        orders.RequestReturnAsync(command.OrderId, command.UserId, command.Reason, cancellationToken);

    public Task<OrderDto?> Handle(RequestRefundCommand command, CancellationToken cancellationToken) =>
        orders.RequestRefundAsync(command.OrderId, command.UserId, command.Reason, cancellationToken);

    public Task<OrderDto?> Handle(AssignCourierCommand command, CancellationToken cancellationToken) =>
        orders.AssignCourierAsync(command.OrderId, command.CourierPartner, command.TrackingNumber, cancellationToken);

    public Task<List<OrderStatusHistoryDto>> Handle(GetOrderTimelineQuery query, CancellationToken cancellationToken) =>
        orders.GetOrderTimelineAsync(query.OrderId, query.UserId, cancellationToken);

    public Task<List<OrderTrackingDto>> Handle(GetOrderTrackingQuery query, CancellationToken cancellationToken) =>
        orders.GetOrderTrackingAsync(query.OrderId, query.UserId, cancellationToken);

    public Task<OrderDto?> Handle(AddTrackingUpdateCommand command, CancellationToken cancellationToken) =>
        orders.AddTrackingUpdateAsync(command.OrderId, command.Status, command.Location, cancellationToken);
}

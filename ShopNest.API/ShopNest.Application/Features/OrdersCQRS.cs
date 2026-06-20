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

public sealed class OrderHandlers(ICartOrderService orders, IAdminService admin) :
    IRequestHandler<CheckoutCommand, OrderDto>,
    IRequestHandler<GetMyOrdersQuery, PagedResult<OrderDto>>,
    IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>,
    IRequestHandler<GetOrderByIdQuery, OrderDto?>,
    IRequestHandler<UpdateOrderStatusCommand, OrderDto?>,
    IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>,
    IRequestHandler<UpdateInventoryCommand, bool>
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
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IShippingService
{
    Task<PagedResult<ShipmentDto>> GetShipmentsAsync(Guid? userId, string? courier, string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<ShipmentDto?> GetShipmentByIdAsync(Guid shipmentId, Guid? userId, CancellationToken cancellationToken);
    Task<ShipmentDto?> GetShipmentByTrackingNumberAsync(string trackingNumber, Guid? userId, CancellationToken cancellationToken);
    Task<List<ShipmentTrackingDto>> GetShipmentTimelineAsync(Guid shipmentId, Guid? userId, CancellationToken cancellationToken);
    Task<ShipmentDto> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken cancellationToken);
    Task<ShipmentDto?> AssignCourierAsync(Guid shipmentId, AssignCourierRequest request, CancellationToken cancellationToken);
    Task<ShipmentDto?> UpdateShipmentStatusAsync(Guid shipmentId, UpdateShipmentStatusRequest request, CancellationToken cancellationToken);
    Task<ShipmentDto?> RescheduleDeliveryAsync(Guid shipmentId, RescheduleDeliveryRequest request, CancellationToken cancellationToken);
    Task<ShipmentDto?> MarkDeliveredAsync(Guid shipmentId, CancellationToken cancellationToken);
    Task<ShipmentDto?> MarkFailedAsync(Guid shipmentId, string reason, CancellationToken cancellationToken);
    Task<List<CourierDto>> GetCouriersAsync(CancellationToken cancellationToken);
    Task<CourierDto> CreateCourierAsync(CreateCourierRequest request, CancellationToken cancellationToken);
}

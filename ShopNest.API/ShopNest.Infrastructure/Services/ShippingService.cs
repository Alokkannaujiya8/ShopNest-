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
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class ShippingService(
    ShopNestDbContext db,
    IHubContext<OrderHub> hub,
    INotificationService notificationService
) : IShippingService
{
    public async Task<PagedResult<ShipmentDto>> GetShipmentsAsync(
        Guid? userId,
        string? courier,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(x => x.Order.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(courier))
        {
            query = query.Where(x => x.Courier != null && x.Courier.Code == courier);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapShipmentToDto).ToList();
        return new PagedResult<ShipmentDto>(dtos, page, pageSize, total);
    }

    public async Task<ShipmentDto?> GetShipmentByIdAsync(Guid shipmentId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .AsNoTracking()
            .Where(x => x.Id == shipmentId);

        if (userId.HasValue)
        {
            query = query.Where(x => x.Order.UserId == userId.Value);
        }

        var shipment = await query.FirstOrDefaultAsync(cancellationToken);
        return shipment == null ? null : MapShipmentToDto(shipment);
    }

    public async Task<ShipmentDto?> GetShipmentByTrackingNumberAsync(string trackingNumber, Guid? userId, CancellationToken cancellationToken)
    {
        var query = db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .AsNoTracking()
            .Where(x => x.TrackingNumber == trackingNumber);

        if (userId.HasValue)
        {
            query = query.Where(x => x.Order.UserId == userId.Value);
        }

        var shipment = await query.FirstOrDefaultAsync(cancellationToken);
        return shipment == null ? null : MapShipmentToDto(shipment);
    }

    public async Task<List<ShipmentTrackingDto>> GetShipmentTimelineAsync(Guid shipmentId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = db.Shipments.AsNoTracking().Where(x => x.Id == shipmentId);
        if (userId.HasValue)
        {
            query = query.Where(x => x.Order.UserId == userId.Value);
        }

        var exists = await query.AnyAsync(cancellationToken);
        if (!exists) return new List<ShipmentTrackingDto>();

        var history = await db.ShipmentTrackings
            .AsNoTracking()
            .Where(x => x.ShipmentId == shipmentId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return history.Select(MapTrackingToDto).ToList();
    }

    public async Task<ShipmentDto> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");

        var shipmentNum = $"SH-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var trackingNum = request.ManualTrackingNumber ?? $"TR-{Random.Shared.Next(100000000, 999999999)}";

        var shipment = new Shipment
        {
            ShipmentNumber = shipmentNum,
            TrackingNumber = trackingNum,
            OrderId = request.OrderId,
            CourierId = request.CourierId,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            ShippingCharges = request.ShippingCharges,
            DeliveryInstructions = request.DeliveryInstructions ?? string.Empty,
            Status = "Shipment Created",
            ShipmentDate = DateTime.UtcNow,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7)
        };

        shipment.TrackingHistory.Add(new ShipmentTracking
        {
            Status = "Shipment Created",
            Location = "Warehouse Hub",
            Description = "Shipment label generated and package packed."
        });

        db.Shipments.Add(shipment);
        
        // Audit log shipment creation
        db.AuditLogs.Add(new AuditLog
        {
            Action = "ShipmentCreated",
            UserId = order.UserId,
            EntityName = "Shipment",
            EntityId = shipment.Id.ToString(),
            Details = $"Shipment {shipmentNum} created for Order {order.OrderNumber} with Tracking {trackingNum}."
        });

        await db.SaveChangesAsync(cancellationToken);

        // Fetch back fully mapped
        var saved = await db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .FirstAsync(x => x.Id == shipment.Id, cancellationToken);

        var dto = MapShipmentToDto(saved);
        await hub.Clients.Group(order.UserId.ToString()).SendAsync("shipmentStatusChanged", dto, cancellationToken);

        await notificationService.SendTemplatedNotificationAsync(
            saved.Order.UserId,
            "ShippingUpdate",
            new Dictionary<string, string>
            {
                { "OrderNumber", saved.Order.OrderNumber },
                { "Status", saved.Status },
                { "CourierName", saved.Courier?.Name ?? "Courier" },
                { "TrackingNumber", saved.TrackingNumber }
            },
            "Shipment",
            saved.Id.ToString(),
            cancellationToken);
        
        return dto;
    }

    public async Task<ShipmentDto?> AssignCourierAsync(Guid shipmentId, AssignCourierRequest request, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .FirstOrDefaultAsync(x => x.Id == shipmentId, cancellationToken);

        if (shipment is null) return null;

        var courier = await db.Couriers.FirstOrDefaultAsync(x => x.Id == request.CourierId, cancellationToken)
            ?? throw new InvalidOperationException("Courier not found.");

        shipment.CourierId = request.CourierId;
        shipment.Courier = courier;
        if (request.ManualTrackingNumber is not null)
        {
            shipment.TrackingNumber = request.ManualTrackingNumber;
        }
        shipment.Status = "Pickup Scheduled";
        shipment.PickupDate = DateTime.UtcNow;

        shipment.TrackingHistory.Add(new ShipmentTracking
        {
            Status = "Pickup Scheduled",
            Location = "Warehouse Hub",
            Description = $"Pickup scheduled with courier: {courier.Name}."
        });

        // Audit log
        db.AuditLogs.Add(new AuditLog
        {
            Action = "CourierAssigned",
            UserId = shipment.Order.UserId,
            EntityName = "Shipment",
            EntityId = shipment.Id.ToString(),
            Details = $"Courier {courier.Name} assigned to Shipment {shipment.ShipmentNumber}."
        });

        await db.SaveChangesAsync(cancellationToken);

        var dto = MapShipmentToDto(shipment);
        await hub.Clients.Group(shipment.Order.UserId.ToString()).SendAsync("shipmentStatusChanged", dto, cancellationToken);
        return dto;
    }

    public async Task<ShipmentDto?> UpdateShipmentStatusAsync(Guid shipmentId, UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .FirstOrDefaultAsync(x => x.Id == shipmentId, cancellationToken);

        if (shipment is null) return null;

        shipment.Status = request.Status;
        shipment.TrackingHistory.Add(new ShipmentTracking
        {
            Status = request.Status,
            Location = request.Location,
            Description = request.Description ?? $"Shipment transitioned to status: {request.Status}."
        });

        // Automatically align order status with transit phases
        if (request.Status.Equals("In Transit", StringComparison.OrdinalIgnoreCase))
        {
            shipment.Order.Status = OrderStatus.Shipped;
            shipment.Order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = shipment.OrderId,
                Status = OrderStatus.Shipped,
                Note = $"Shipment in transit. Tracking #: {shipment.TrackingNumber}",
                ChangedBy = "ShippingGateway"
            });
        }
        else if (request.Status.Equals("Out For Delivery", StringComparison.OrdinalIgnoreCase))
        {
            shipment.Order.Status = OrderStatus.OutForDelivery;
            shipment.Order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = shipment.OrderId,
                Status = OrderStatus.OutForDelivery,
                Note = "Shipment out for delivery.",
                ChangedBy = "ShippingGateway"
            });
        }

        db.AuditLogs.Add(new AuditLog
        {
            Action = "ShipmentStatusChanged",
            UserId = shipment.Order.UserId,
            EntityName = "Shipment",
            EntityId = shipment.Id.ToString(),
            Details = $"Shipment {shipment.ShipmentNumber} status updated to {request.Status}."
        });

        await db.SaveChangesAsync(cancellationToken);

        var dto = MapShipmentToDto(shipment);
        await hub.Clients.Group(shipment.Order.UserId.ToString()).SendAsync("shipmentStatusChanged", dto, cancellationToken);

        await notificationService.SendTemplatedNotificationAsync(
            shipment.Order.UserId,
            "ShippingUpdate",
            new Dictionary<string, string>
            {
                { "OrderNumber", shipment.Order.OrderNumber },
                { "Status", shipment.Status },
                { "CourierName", shipment.Courier?.Name ?? "Courier" },
                { "TrackingNumber", shipment.TrackingNumber }
            },
            "Shipment",
            shipment.Id.ToString(),
            cancellationToken);
        
        return dto;
    }

    public async Task<ShipmentDto?> RescheduleDeliveryAsync(Guid shipmentId, RescheduleDeliveryRequest request, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .FirstOrDefaultAsync(x => x.Id == shipmentId, cancellationToken);

        if (shipment is null) return null;

        shipment.Status = "Delivery Rescheduled";
        shipment.EstimatedDeliveryDate = request.NewEstimatedDeliveryDate;

        shipment.TrackingHistory.Add(new ShipmentTracking
        {
            Status = "Delivery Rescheduled",
            Location = "Sorting Facility",
            Description = $"Delivery rescheduled to {request.NewEstimatedDeliveryDate:yyyy-MM-dd}. Reason: {request.Reason}"
        });

        await db.SaveChangesAsync(cancellationToken);

        var dto = MapShipmentToDto(shipment);
        await hub.Clients.Group(shipment.Order.UserId.ToString()).SendAsync("shipmentStatusChanged", dto, cancellationToken);

        await notificationService.SendTemplatedNotificationAsync(
            shipment.Order.UserId,
            "ShippingUpdate",
            new Dictionary<string, string>
            {
                { "OrderNumber", shipment.Order.OrderNumber },
                { "Status", shipment.Status },
                { "CourierName", shipment.Courier?.Name ?? "Courier" },
                { "TrackingNumber", shipment.TrackingNumber }
            },
            "Shipment",
            shipment.Id.ToString(),
            cancellationToken);
        
        return dto;
    }

    public async Task<ShipmentDto?> MarkDeliveredAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .FirstOrDefaultAsync(x => x.Id == shipmentId, cancellationToken);

        if (shipment is null) return null;

        shipment.Status = "Delivered";
        shipment.DeliveredDate = DateTime.UtcNow;

        shipment.TrackingHistory.Add(new ShipmentTracking
        {
            Status = "Delivered",
            Location = "Customer Address",
            Description = "Shipment delivered and handed over to consignee."
        });

        // Update underlying Order status to Delivered
        shipment.Order.Status = OrderStatus.Delivered;
        shipment.Order.DeliveredDate = DateTime.UtcNow;
        shipment.Order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = shipment.OrderId,
            Status = OrderStatus.Delivered,
            Note = "Package successfully delivered.",
            ChangedBy = "Courier"
        });

        db.AuditLogs.Add(new AuditLog
        {
            Action = "DeliveryCompleted",
            UserId = shipment.Order.UserId,
            EntityName = "Shipment",
            EntityId = shipment.Id.ToString(),
            Details = $"Shipment {shipment.ShipmentNumber} delivered successfully."
        });

        await db.SaveChangesAsync(cancellationToken);

        var dto = MapShipmentToDto(shipment);
        await hub.Clients.Group(shipment.Order.UserId.ToString()).SendAsync("shipmentStatusChanged", dto, cancellationToken);

        await notificationService.SendTemplatedNotificationAsync(
            shipment.Order.UserId,
            "ShippingUpdate",
            new Dictionary<string, string>
            {
                { "OrderNumber", shipment.Order.OrderNumber },
                { "Status", shipment.Status },
                { "CourierName", shipment.Courier?.Name ?? "Courier" },
                { "TrackingNumber", shipment.TrackingNumber }
            },
            "Shipment",
            shipment.Id.ToString(),
            cancellationToken);
        
        return dto;
    }

    public async Task<ShipmentDto?> MarkFailedAsync(Guid shipmentId, string reason, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments
            .Include(x => x.Order)
            .Include(x => x.Courier)
            .Include(x => x.TrackingHistory)
            .FirstOrDefaultAsync(x => x.Id == shipmentId, cancellationToken);

        if (shipment is null) return null;

        shipment.Status = "Delivery Failed";
        shipment.TrackingHistory.Add(new ShipmentTracking
        {
            Status = "Delivery Failed",
            Location = "Transit",
            Description = $"Delivery failed. Reason: {reason}."
        });

        db.AuditLogs.Add(new AuditLog
        {
            Action = "DeliveryFailed",
            UserId = shipment.Order.UserId,
            EntityName = "Shipment",
            EntityId = shipment.Id.ToString(),
            Details = $"Shipment {shipment.ShipmentNumber} delivery failed: {reason}."
        });

        await db.SaveChangesAsync(cancellationToken);

        var dto = MapShipmentToDto(shipment);
        await hub.Clients.Group(shipment.Order.UserId.ToString()).SendAsync("shipmentStatusChanged", dto, cancellationToken);
        return dto;
    }

    public async Task<List<CourierDto>> GetCouriersAsync(CancellationToken cancellationToken)
    {
        var couriers = await db.Couriers.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);
        return couriers.Select(MapCourierToDto).ToList();
    }

    public async Task<CourierDto> CreateCourierAsync(CreateCourierRequest request, CancellationToken cancellationToken)
    {
        var courier = new Courier
        {
            Name = request.Name,
            Code = request.Code,
            Contact = request.Contact,
            Website = request.Website,
            EstimatedDeliveryTime = request.EstimatedDeliveryTime,
            Status = "Active"
        };

        db.Couriers.Add(courier);
        await db.SaveChangesAsync(cancellationToken);
        return MapCourierToDto(courier);
    }

    // Static Mapper Helpers
    private static CourierDto MapCourierToDto(Courier c) => new(
        c.Id,
        c.Name,
        c.Code,
        c.Contact,
        c.Website,
        c.Status,
        c.EstimatedDeliveryTime
    );

    private static ShipmentTrackingDto MapTrackingToDto(ShipmentTracking t) => new(
        t.Id,
        t.ShipmentId,
        t.Status,
        t.Location,
        t.Description,
        t.CreatedAtUtc
    );

    private static ShipmentDto MapShipmentToDto(Shipment s) => new(
        s.Id,
        s.ShipmentNumber,
        s.TrackingNumber,
        s.OrderId,
        s.Order.OrderNumber,
        s.Courier == null ? null : MapCourierToDto(s.Courier),
        s.ShippingAddress,
        s.BillingAddress,
        s.ShipmentDate,
        s.PickupDate,
        s.EstimatedDeliveryDate,
        s.DeliveredDate,
        s.ShippingCharges,
        s.DeliveryInstructions,
        s.Status,
        s.Notes,
        s.TrackingHistory.Select(MapTrackingToDto).ToList()
    );
}

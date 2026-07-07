using System;
using System.Collections.Generic;

namespace ShopNest.Application.Dtos;

public sealed record CourierDto(
    Guid Id,
    string Name,
    string Code,
    string Contact,
    string Website,
    string Status,
    string EstimatedDeliveryTime
);

public sealed record ShipmentTrackingDto(
    Guid Id,
    Guid ShipmentId,
    string Status,
    string Location,
    string Description,
    DateTime CreatedAtUtc
);

public sealed record ShipmentDto(
    Guid Id,
    string ShipmentNumber,
    string TrackingNumber,
    Guid OrderId,
    string OrderNumber,
    CourierDto? Courier,
    string ShippingAddress,
    string BillingAddress,
    DateTime? ShipmentDate,
    DateTime? PickupDate,
    DateTime? EstimatedDeliveryDate,
    DateTime? DeliveredDate,
    decimal ShippingCharges,
    string DeliveryInstructions,
    string Status,
    string Notes,
    IReadOnlyList<ShipmentTrackingDto> TrackingHistory
);

public sealed record CreateCourierRequest(
    string Name,
    string Code,
    string Contact,
    string Website,
    string EstimatedDeliveryTime
);

public sealed record CreateShipmentRequest(
    Guid OrderId,
    Guid? CourierId,
    string? ManualTrackingNumber,
    string ShippingAddress,
    string BillingAddress,
    decimal ShippingCharges,
    string? DeliveryInstructions
);

public sealed record AssignCourierRequest(Guid CourierId, string? ManualTrackingNumber);
public sealed record UpdateShipmentStatusRequest(string Status, string Location, string? Description);
public sealed record RescheduleDeliveryRequest(DateTime NewEstimatedDeliveryDate, string Reason);

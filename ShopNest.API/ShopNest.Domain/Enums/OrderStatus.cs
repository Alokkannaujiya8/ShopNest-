namespace ShopNest.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
    Confirmed = 5,
    PaymentPending = 6,
    PaymentCompleted = 7,
    Processing = 8,
    Packed = 9,
    ReadyToShip = 10,
    OutForDelivery = 11,
    ReturnRequested = 12,
    Returned = 13,
    RefundRequested = 14,
    Refunded = 15
}

using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class OrderShippingAddress : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? AlternateMobile { get; set; }
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? Landmark { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
}

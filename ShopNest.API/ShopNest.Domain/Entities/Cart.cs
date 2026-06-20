using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public List<CartItem> Items { get; set; } = [];
}

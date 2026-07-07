using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class WishlistItem : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

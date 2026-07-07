using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class SearchHistory : BaseEntity
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string QueryText { get; set; } = string.Empty;
}

using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ReviewImage : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
}

using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ReviewHelpfulVote : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ProductQuestion : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string QuestionText { get; set; } = string.Empty;
    public List<ProductAnswer> Answers { get; set; } = [];
}

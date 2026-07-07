using System;
using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class ProductAnswer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public ProductQuestion Question { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string AnswerText { get; set; } = string.Empty;
    public bool IsAdminOrSeller { get; set; }
}

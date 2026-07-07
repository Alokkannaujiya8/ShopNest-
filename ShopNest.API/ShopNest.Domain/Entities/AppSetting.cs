using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class AppSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

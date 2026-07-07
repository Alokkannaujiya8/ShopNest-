using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class PermissionGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Permission> Permissions { get; set; } = [];
}

using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<AppUserRole> UserRoles { get; set; } = [];
    public List<RolePermission> RolePermissions { get; set; } = [];
}

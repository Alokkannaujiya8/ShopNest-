using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Users.Create
    public string DisplayName { get; set; } = string.Empty; // Create User
    public string? Description { get; set; }
    
    public Guid PermissionGroupId { get; set; }
    public PermissionGroup PermissionGroup { get; set; } = null!;
    
    public List<RolePermission> RolePermissions { get; set; } = [];
    public List<UserPermission> UserPermissions { get; set; } = [];
}

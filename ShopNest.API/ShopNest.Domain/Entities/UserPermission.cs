using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class UserPermission : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}

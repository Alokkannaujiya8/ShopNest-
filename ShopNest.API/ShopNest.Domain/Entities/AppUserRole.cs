using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class AppUserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

using ShopNest.Domain.Common;

namespace ShopNest.Domain.Entities;

public sealed class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? ProfilePicturePublicId { get; set; }
}

using Microsoft.AspNetCore.Http;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<UserProfileDto?> UploadProfilePictureAsync(Guid userId, IFormFile file, CancellationToken cancellationToken);
    Task<bool> DeleteProfilePictureAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserAddressDto>> GetAddressesAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserAddressDto> AddAddressAsync(Guid userId, AddAddressRequest request, CancellationToken cancellationToken);
    Task<UserAddressDto?> UpdateAddressAsync(Guid userId, Guid addressId, AddAddressRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken);
    Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderDto>> GetOrderHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> AddToWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<bool> MoveWishlistToCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
}

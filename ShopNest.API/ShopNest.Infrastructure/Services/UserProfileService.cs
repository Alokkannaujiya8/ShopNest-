using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class UserProfileService(
    ShopNestDbContext db,
    IImageStorageService imageStorage,
    ICartOrderService cartOrderService,
    IMapper mapper) : IUserProfileService
{
    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null) return null;

        if (user.Profile is null)
        {
            user.Profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(user.Profile);
            await db.SaveChangesAsync(cancellationToken);
        }

        return mapper.Map<UserProfileDto>(user.Profile);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null) return null;

        if (user.Profile is null)
        {
            user.Profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(user.Profile);
        }

        user.FullName = request.FullName.Trim();
        user.MobileNumber = request.MobileNumber.Trim();
        user.Profile.DateOfBirth = request.DateOfBirth;
        user.Profile.Gender = request.Gender;
        user.Profile.Bio = request.Bio?.Trim();
        user.Profile.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserProfileDto>(user.Profile);
    }

    public async Task<UserProfileDto?> UploadProfilePictureAsync(Guid userId, IFormFile file, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null) return null;

        if (user.Profile is null)
        {
            user.Profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(user.Profile);
        }

        // Delete previous picture if exists
        if (!string.IsNullOrWhiteSpace(user.Profile.ProfilePicturePublicId))
        {
            try
            {
                await imageStorage.DeleteAsync(user.Profile.ProfilePicturePublicId, cancellationToken);
            }
            catch
            {
                // Fail silently and proceed with upload
            }
        }

        await using var stream = file.OpenReadStream();
        var uploadResult = await imageStorage.UploadAsync(stream, file.FileName, cancellationToken);

        user.Profile.ProfilePictureUrl = uploadResult.Url;
        user.Profile.ProfilePicturePublicId = string.IsNullOrEmpty(uploadResult.PublicId) ? uploadResult.Url : uploadResult.PublicId;
        user.Profile.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserProfileDto>(user.Profile);
    }

    public async Task<bool> DeleteProfilePictureAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user?.Profile is null || string.IsNullOrWhiteSpace(user.Profile.ProfilePictureUrl))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(user.Profile.ProfilePicturePublicId))
        {
            await imageStorage.DeleteAsync(user.Profile.ProfilePicturePublicId, cancellationToken);
        }

        user.Profile.ProfilePictureUrl = null;
        user.Profile.ProfilePicturePublicId = null;
        user.Profile.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<UserAddressDto>> GetAddressesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var addresses = await db.UserAddresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<UserAddressDto>>(addresses);
    }

    public async Task<UserAddressDto> AddAddressAsync(Guid userId, AddAddressRequest request, CancellationToken cancellationToken)
    {
        var address = mapper.Map<UserAddress>(request);
        address.UserId = userId;

        var hasAnyAddress = await db.UserAddresses.AnyAsync(x => x.UserId == userId, cancellationToken);
        if (!hasAnyAddress)
        {
            address.IsDefault = true;
        }
        else if (address.IsDefault)
        {
            await ResetDefaultAddressesAsync(userId, cancellationToken);
        }

        db.UserAddresses.Add(address);
        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserAddressDto>(address);
    }

    public async Task<UserAddressDto?> UpdateAddressAsync(Guid userId, Guid addressId, AddAddressRequest request, CancellationToken cancellationToken)
    {
        var address = await db.UserAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId, cancellationToken);

        if (address is null) return null;

        mapper.Map(request, address);
        address.UpdatedAtUtc = DateTime.UtcNow;

        if (request.IsDefault)
        {
            await ResetDefaultAddressesAsync(userId, cancellationToken);
            address.IsDefault = true;
        }
        else
        {
            // Ensure at least one default address exists
            var isAnyOtherDefault = await db.UserAddresses
                .AnyAsync(x => x.UserId == userId && x.Id != addressId && x.IsDefault, cancellationToken);

            if (!isAnyOtherDefault)
            {
                address.IsDefault = true; // Stay default if no other default exists
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserAddressDto>(address);
    }

    public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
    {
        var address = await db.UserAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId, cancellationToken);

        if (address is null) return false;

        var wasDefault = address.IsDefault;
        db.UserAddresses.Remove(address);
        await db.SaveChangesAsync(cancellationToken);

        if (wasDefault)
        {
            var nextAddress = await db.UserAddresses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextAddress is not null)
            {
                nextAddress.IsDefault = true;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
    {
        var address = await db.UserAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId, cancellationToken);

        if (address is null) return false;

        await ResetDefaultAddressesAsync(userId, cancellationToken);
        address.IsDefault = true;
        address.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken);
        if (user is null) return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Incorrect current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrderHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await cartOrderService.GetOrdersAsync(userId, 1, 100, cancellationToken);
        return result.Items;
    }

    public async Task<IReadOnlyList<ProductDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wishlistItems = await db.WishlistItems
            .AsNoTracking()
            .Include(x => x.Product)
            .ThenInclude(x => x.Category)
            .Include(x => x.Product)
            .ThenInclude(x => x.Images)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var products = wishlistItems.Select(x => x.Product).ToList();
        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<bool> AddToWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var alreadyExists = await db.WishlistItems
            .AnyAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);

        if (alreadyExists) return true;

        var productExists = await db.Products.AnyAsync(x => x.Id == productId, cancellationToken);
        if (!productExists)
        {
            throw new InvalidOperationException("Product not found.");
        }

        var item = new WishlistItem { UserId = userId, ProductId = productId };
        db.WishlistItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var item = await db.WishlistItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);

        if (item is null) return true;

        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MoveWishlistToCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var item = await db.WishlistItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);

        if (item is null) return false;

        // Add to cart
        await cartOrderService.AddToCartAsync(userId, new AddCartItemRequest(productId, 1), cancellationToken);

        // Remove from wishlist
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ResetDefaultAddressesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var defaults = await db.UserAddresses
            .Where(x => x.UserId == userId && x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var def in defaults)
        {
            def.IsDefault = false;
            def.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}

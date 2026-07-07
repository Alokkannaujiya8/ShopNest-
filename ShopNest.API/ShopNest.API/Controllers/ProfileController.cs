using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Profile;

namespace ShopNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public sealed class ProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var profile = await mediator.Send(new GetProfileQuery(userId), cancellationToken);
        if (profile is null) return NotFound(ApiResponse.FailureResult("User profile not found."));
        return Ok(ApiResponse<UserProfileDto>.SuccessResult(profile));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var profile = await mediator.Send(new UpdateProfileCommand(userId, request), cancellationToken);
        if (profile is null) return NotFound(ApiResponse.FailureResult("User profile not found."));
        return Ok(ApiResponse<UserProfileDto>.SuccessResult(profile, "Profile updated successfully."));
    }

    [HttpPost("picture")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UploadProfilePicture(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (file is null || file.Length == 0) return BadRequest(ApiResponse.FailureResult("Please select a profile image."));

        // Validate file extensions
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(ApiResponse.FailureResult("Only .jpg, .jpeg, .png and .gif images are allowed."));
        }

        // Validate file size (max 2MB)
        if (file.Length > 2 * 1024 * 1024)
        {
            return BadRequest(ApiResponse.FailureResult("Image size must not exceed 2MB."));
        }

        var profile = await mediator.Send(new UploadProfilePictureCommand(userId, file), cancellationToken);
        if (profile is null) return NotFound(ApiResponse.FailureResult("User profile not found."));
        return Ok(ApiResponse<UserProfileDto>.SuccessResult(profile, "Profile picture uploaded successfully."));
    }

    [HttpDelete("picture")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProfilePicture(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new DeleteProfilePictureCommand(userId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Profile picture removed successfully."));
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserAddressDto>>>> GetAddresses(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var addresses = await mediator.Send(new GetAddressesQuery(userId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserAddressDto>>.SuccessResult(addresses));
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<ApiResponse<UserAddressDto>>> AddAddress(AddAddressRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var address = await mediator.Send(new AddAddressCommand(userId, request), cancellationToken);
        return Ok(ApiResponse<UserAddressDto>.SuccessResult(address, "Address added successfully."));
    }

    [HttpPut("addresses/{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserAddressDto>>> UpdateAddress(Guid id, AddAddressRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var address = await mediator.Send(new UpdateAddressCommand(userId, id, request), cancellationToken);
        if (address is null) return NotFound(ApiResponse.FailureResult("Address not found."));
        return Ok(ApiResponse<UserAddressDto>.SuccessResult(address, "Address updated successfully."));
    }

    [HttpDelete("addresses/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new DeleteAddressCommand(userId, id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Address not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Address deleted successfully."));
    }

    [HttpPut("addresses/{id:guid}/default")]
    public async Task<ActionResult<ApiResponse<bool>>> SetDefaultAddress(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new SetDefaultAddressCommand(userId, id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Address not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Default address updated successfully."));
    }

    [HttpPut("password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(ChangeProfilePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new ChangeProfilePasswordCommand(userId, request), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Password changed successfully."));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderDto>>>> GetOrderHistory(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var orders = await mediator.Send(new GetOrderHistoryQuery(userId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.SuccessResult(orders));
    }

    [HttpGet("wishlist")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductDto>>>> GetWishlist(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var products = await mediator.Send(new GetWishlistQuery(userId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.SuccessResult(products));
    }

    [HttpPost("wishlist/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> AddToWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new AddToWishlistCommand(userId, productId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product added to wishlist."));
    }

    [HttpDelete("wishlist/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new RemoveFromWishlistCommand(userId, productId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product removed from wishlist."));
    }

    [HttpPost("wishlist/{productId:guid}/move-to-cart")]
    public async Task<ActionResult<ApiResponse<bool>>> MoveWishlistToCart(Guid productId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await mediator.Send(new MoveWishlistToCartCommand(userId, productId), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found in wishlist."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product moved to cart successfully."));
    }
}

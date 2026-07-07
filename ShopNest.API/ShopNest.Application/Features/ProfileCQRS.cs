using MediatR;
using Microsoft.AspNetCore.Http;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Profile;

// Queries
public sealed record GetProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;
public sealed record GetAddressesQuery(Guid UserId) : IRequest<IReadOnlyList<UserAddressDto>>;
public sealed record GetOrderHistoryQuery(Guid UserId) : IRequest<IReadOnlyList<OrderDto>>;
public sealed record GetWishlistQuery(Guid UserId) : IRequest<IReadOnlyList<ProductDto>>;

// Commands
public sealed record UpdateProfileCommand(Guid UserId, UpdateProfileRequest Request) : IRequest<UserProfileDto?>;
public sealed record UploadProfilePictureCommand(Guid UserId, IFormFile File) : IRequest<UserProfileDto?>;
public sealed record DeleteProfilePictureCommand(Guid UserId) : IRequest<bool>;
public sealed record AddAddressCommand(Guid UserId, AddAddressRequest Request) : IRequest<UserAddressDto>;
public sealed record UpdateAddressCommand(Guid UserId, Guid AddressId, AddAddressRequest Request) : IRequest<UserAddressDto?>;
public sealed record DeleteAddressCommand(Guid UserId, Guid AddressId) : IRequest<bool>;
public sealed record SetDefaultAddressCommand(Guid UserId, Guid AddressId) : IRequest<bool>;
public sealed record ChangeProfilePasswordCommand(Guid UserId, ChangeProfilePasswordRequest Request) : IRequest<bool>;
public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<bool>;
public sealed record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest<bool>;
public sealed record MoveWishlistToCartCommand(Guid UserId, Guid ProductId) : IRequest<bool>;

// Handlers
public sealed class ProfileHandlers(IUserProfileService profileService) :
    IRequestHandler<GetProfileQuery, UserProfileDto?>,
    IRequestHandler<GetAddressesQuery, IReadOnlyList<UserAddressDto>>,
    IRequestHandler<GetOrderHistoryQuery, IReadOnlyList<OrderDto>>,
    IRequestHandler<GetWishlistQuery, IReadOnlyList<ProductDto>>,
    IRequestHandler<UpdateProfileCommand, UserProfileDto?>,
    IRequestHandler<UploadProfilePictureCommand, UserProfileDto?>,
    IRequestHandler<DeleteProfilePictureCommand, bool>,
    IRequestHandler<AddAddressCommand, UserAddressDto>,
    IRequestHandler<UpdateAddressCommand, UserAddressDto?>,
    IRequestHandler<DeleteAddressCommand, bool>,
    IRequestHandler<SetDefaultAddressCommand, bool>,
    IRequestHandler<ChangeProfilePasswordCommand, bool>,
    IRequestHandler<AddToWishlistCommand, bool>,
    IRequestHandler<RemoveFromWishlistCommand, bool>,
    IRequestHandler<MoveWishlistToCartCommand, bool>
{
    public Task<UserProfileDto?> Handle(GetProfileQuery query, CancellationToken cancellationToken) =>
        profileService.GetProfileAsync(query.UserId, cancellationToken);

    public Task<IReadOnlyList<UserAddressDto>> Handle(GetAddressesQuery query, CancellationToken cancellationToken) =>
        profileService.GetAddressesAsync(query.UserId, cancellationToken);

    public Task<IReadOnlyList<OrderDto>> Handle(GetOrderHistoryQuery query, CancellationToken cancellationToken) =>
        profileService.GetOrderHistoryAsync(query.UserId, cancellationToken);

    public Task<IReadOnlyList<ProductDto>> Handle(GetWishlistQuery query, CancellationToken cancellationToken) =>
        profileService.GetWishlistAsync(query.UserId, cancellationToken);

    public Task<UserProfileDto?> Handle(UpdateProfileCommand command, CancellationToken cancellationToken) =>
        profileService.UpdateProfileAsync(command.UserId, command.Request, cancellationToken);

    public Task<UserProfileDto?> Handle(UploadProfilePictureCommand command, CancellationToken cancellationToken) =>
        profileService.UploadProfilePictureAsync(command.UserId, command.File, cancellationToken);

    public Task<bool> Handle(DeleteProfilePictureCommand command, CancellationToken cancellationToken) =>
        profileService.DeleteProfilePictureAsync(command.UserId, cancellationToken);

    public Task<UserAddressDto> Handle(AddAddressCommand command, CancellationToken cancellationToken) =>
        profileService.AddAddressAsync(command.UserId, command.Request, cancellationToken);

    public Task<UserAddressDto?> Handle(UpdateAddressCommand command, CancellationToken cancellationToken) =>
        profileService.UpdateAddressAsync(command.UserId, command.AddressId, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteAddressCommand command, CancellationToken cancellationToken) =>
        profileService.DeleteAddressAsync(command.UserId, command.AddressId, cancellationToken);

    public Task<bool> Handle(SetDefaultAddressCommand command, CancellationToken cancellationToken) =>
        profileService.SetDefaultAddressAsync(command.UserId, command.AddressId, cancellationToken);

    public Task<bool> Handle(ChangeProfilePasswordCommand command, CancellationToken cancellationToken) =>
        profileService.ChangePasswordAsync(command.UserId, command.Request.CurrentPassword, command.Request.NewPassword, cancellationToken);

    public Task<bool> Handle(AddToWishlistCommand command, CancellationToken cancellationToken) =>
        profileService.AddToWishlistAsync(command.UserId, command.ProductId, cancellationToken);

    public Task<bool> Handle(RemoveFromWishlistCommand command, CancellationToken cancellationToken) =>
        profileService.RemoveFromWishlistAsync(command.UserId, command.ProductId, cancellationToken);

    public Task<bool> Handle(MoveWishlistToCartCommand command, CancellationToken cancellationToken) =>
        profileService.MoveWishlistToCartAsync(command.UserId, command.ProductId, cancellationToken);
}

namespace ShopNest.Application.Dtos;

public sealed record UserProfileDto(
    Guid UserId, 
    string FullName, 
    string Email, 
    string MobileNumber, 
    DateTime? DateOfBirth, 
    string? Gender, 
    string? Bio, 
    string? ProfilePictureUrl
);

public sealed record UpdateProfileRequest(
    string FullName, 
    string MobileNumber, 
    DateTime? DateOfBirth, 
    string? Gender, 
    string? Bio
);

public sealed record UserAddressDto(
    Guid Id,
    string FullName,
    string MobileNumber,
    string? AlternateMobile,
    string Country,
    string State,
    string City,
    string Area,
    string? Landmark,
    string PostalCode,
    string AddressLine1,
    string? AddressLine2,
    string AddressType,
    string? Email,
    string? DeliveryInstructions,
    bool IsDefault
);

public sealed record AddAddressRequest(
    string FullName,
    string MobileNumber,
    string? AlternateMobile,
    string Country,
    string State,
    string City,
    string Area,
    string? Landmark,
    string PostalCode,
    string AddressLine1,
    string? AddressLine2,
    string AddressType,
    string? Email,
    string? DeliveryInstructions,
    bool IsDefault
);

public sealed record ChangeProfilePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

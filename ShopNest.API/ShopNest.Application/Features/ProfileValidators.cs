using FluentValidation;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Features.Profile;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(150).WithMessage("Full name must not exceed 150 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid mobile number. Must be 10 to 15 digits.");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters.");

        RuleFor(x => x.Gender)
            .Must(g => string.IsNullOrWhiteSpace(g) || g == "Male" || g == "Female" || g == "Other")
            .WithMessage("Gender must be 'Male', 'Female', or 'Other'.");
    }
}

public sealed class AddAddressRequestValidator : AbstractValidator<AddAddressRequest>
{
    public AddAddressRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Recipient name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid mobile number. Must be 10 to 15 digits.");

        RuleFor(x => x.AlternateMobile)
            .Matches(@"^\+?[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.AlternateMobile))
            .WithMessage("Invalid alternate mobile number. Must be 10 to 15 digits.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Area)
            .NotEmpty().WithMessage("Area is required.")
            .MaximumLength(150).WithMessage("Area must not exceed 150 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .Matches(@"^[a-zA-Z0-9 -]{5,10}$").WithMessage("Invalid postal code. Must be 5 to 10 alphanumeric characters.");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address Line 1 is required.")
            .MaximumLength(250).WithMessage("Address must not exceed 250 characters.");

        RuleFor(x => x.AddressType)
            .NotEmpty().WithMessage("Address type is required.")
            .Must(t => t == "Home" || t == "Office" || t == "Other")
            .WithMessage("Address type must be 'Home', 'Office', or 'Other'.");
    }
}

public sealed class ChangeProfilePasswordRequestValidator : AbstractValidator<ChangeProfilePasswordRequest>
{
    public ChangeProfilePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

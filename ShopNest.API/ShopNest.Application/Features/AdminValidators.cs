using FluentValidation;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Features.Admin;

public sealed class CreateAdminUserRequestValidator : AbstractValidator<CreateAdminUserRequest>
{
    public CreateAdminUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(160).WithMessage("Full name must not exceed 160 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid mobile number. Must be 10 to 15 digits.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => r == "Admin" || r == "Customer" || r == "Seller")
            .WithMessage("Role must be 'Admin', 'Customer', or 'Seller'.");
    }
}

public sealed class UpdateAdminUserRequestValidator : AbstractValidator<UpdateAdminUserRequest>
{
    public UpdateAdminUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(160).WithMessage("Full name must not exceed 160 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid mobile number. Must be 10 to 15 digits.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => r == "Admin" || r == "Customer" || r == "Seller")
            .WithMessage("Role must be 'Admin', 'Customer', or 'Seller'.");
    }
}

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name code is required.")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

public sealed class AdminResetPasswordRequestValidator : AbstractValidator<AdminResetPasswordRequest>
{
    public AdminResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

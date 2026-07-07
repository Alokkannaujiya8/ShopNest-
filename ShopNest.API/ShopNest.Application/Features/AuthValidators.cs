using FluentValidation;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Features.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(150).WithMessage("Full name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid mobile number. Must be 10 to 15 digits (optionally starting with '+').");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must accept the terms and conditions.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role selection.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class VerifyEmailOtpRequestValidator : AbstractValidator<VerifyEmailOtpRequest>
{
    public VerifyEmailOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(6).WithMessage("OTP must be exactly 6 digits.")
            .Matches(@"^[0-9]+$").WithMessage("OTP must contain only numbers.");
    }
}

public sealed class ResendEmailOtpRequestValidator : AbstractValidator<ResendEmailOtpRequest>
{
    public ResendEmailOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(6).WithMessage("OTP must be exactly 6 digits.")
            .Matches(@"^[0-9]+$").WithMessage("OTP must contain only numbers.");

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

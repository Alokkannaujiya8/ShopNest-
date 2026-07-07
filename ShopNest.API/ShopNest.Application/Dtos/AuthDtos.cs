using ShopNest.Domain.Enums;

namespace ShopNest.Application.Dtos;

public sealed record RegisterRequest(string FullName, string Email, string MobileNumber, string Password, string ConfirmPassword, bool AcceptTerms, UserRole Role = UserRole.Customer);
public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);
public sealed record RefreshTokenRequest(string? RefreshToken);
public sealed record AuthResponse(Guid UserId, string FullName, string Email, string Role, string AccessToken, string? RefreshToken, DateTime AccessTokenExpiresAtUtc);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Otp, string NewPassword, string ConfirmPassword);
public sealed record VerifyEmailOtpRequest(string Email, string Otp);
public sealed record ResendEmailOtpRequest(string Email);

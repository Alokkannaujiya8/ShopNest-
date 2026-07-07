using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
    Task<AuthResponse> VerifyEmailOtpAsync(string email, string otp, CancellationToken cancellationToken);
    Task<bool> ResendEmailOtpAsync(string email, CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}

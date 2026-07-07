using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;
using ShopNest.Infrastructure.Settings;

namespace ShopNest.Infrastructure.Services;

public sealed class AuthService(
    ShopNestDbContext db, 
    IOptions<JwtSettings> jwtOptions, 
    IHttpContextAccessor httpContextAccessor,
    INotificationService notificationService) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var mobile = request.MobileNumber.Trim();
        if (!string.IsNullOrEmpty(mobile) && await db.Users.AnyAsync(x => x.MobileNumber == mobile, cancellationToken))
        {
            throw new InvalidOperationException("Mobile number already registered.");
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            MobileNumber = request.MobileNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsEmailVerified = false
        };

        // Generate 6-digit email OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString("D6");
        user.EmailVerificationOtpHash = Hash(otp);
        user.EmailVerificationOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(5); // 5-minute expiry
        user.EmailVerificationOtpRetryCount = 0;

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        // Send OTP via templated notification
        await notificationService.SendTemplatedNotificationAsync(
            user.Id,
            "OtpEmail",
            new Dictionary<string, string> { { "Otp", otp } },
            "AppUser",
            user.Id.ToString(),
            cancellationToken);

        return true;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Enforce email verification
        if (!user.IsEmailVerified)
        {
            throw new EmailNotVerifiedException(user.Email);
        }

        return await IssueTokensAsync(user, request.RememberMe, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var token = request.RefreshToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            // Fallback to cookie
            token = httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Refresh token is missing.");
        }

        var tokenHash = Hash(token);
        var stored = await db.RefreshTokens.Include(x => x.User).ThenInclude(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!stored.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(stored.User, false, cancellationToken);
    }

    public async Task<AuthResponse> VerifyEmailOtpAsync(string email, string otp, CancellationToken cancellationToken)
    {
        var mail = email.Trim().ToLowerInvariant();
        var user = await db.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Email == mail, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (user.IsEmailVerified)
        {
            return await IssueTokensAsync(user, false, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(user.EmailVerificationOtpHash) || 
            !user.EmailVerificationOtpExpiresAtUtc.HasValue || 
            user.EmailVerificationOtpExpiresAtUtc.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("OTP has expired. Please request a new OTP.");
        }

        if (user.EmailVerificationOtpRetryCount >= 3)
        {
            throw new InvalidOperationException("OTP retry limit exceeded. Please request a new OTP.");
        }

        var inputHash = Hash(otp.Trim());
        if (user.EmailVerificationOtpHash != inputHash)
        {
            user.EmailVerificationOtpRetryCount++;
            await db.SaveChangesAsync(cancellationToken);

            var attemptsLeft = 3 - user.EmailVerificationOtpRetryCount;
            if (attemptsLeft <= 0)
            {
                throw new InvalidOperationException("Invalid OTP. Retry limit exceeded. Please request a new OTP.");
            }
            throw new InvalidOperationException($"Invalid OTP. {attemptsLeft} attempts remaining.");
        }

        // Email verified successfully
        user.IsEmailVerified = true;
        user.EmailVerificationOtpHash = null;
        user.EmailVerificationOtpExpiresAtUtc = null;
        user.EmailVerificationOtpRetryCount = 0;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        // Send Welcome Email
        await notificationService.SendTemplatedNotificationAsync(
            user.Id,
            "WelcomeEmail",
            new Dictionary<string, string>(),
            "AppUser",
            user.Id.ToString(),
            cancellationToken);

        return await IssueTokensAsync(user, false, cancellationToken);
    }

    public async Task<bool> ResendEmailOtpAsync(string email, CancellationToken cancellationToken)
    {
        var mail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == mail, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email is already verified.");
        }

        // Generate new 6-digit email OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString("D6");
        user.EmailVerificationOtpHash = Hash(otp);
        user.EmailVerificationOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(5); // 5-minute expiry
        user.EmailVerificationOtpRetryCount = 0;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        // Send OTP via templated notification
        await notificationService.SendTemplatedNotificationAsync(
            user.Id,
            "OtpEmail",
            new Dictionary<string, string> { { "Otp", otp } },
            "AppUser",
            user.Id.ToString(),
            cancellationToken);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var mail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == mail, cancellationToken);
        
        if (user is null)
        {
            return true; // Security: prevent email harvesting
        }

        // Generate 6-digit OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString("D6");
        user.PasswordResetOtpHash = Hash(otp);
        user.PasswordResetOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(5); // 5-minute expiry
        
        await db.SaveChangesAsync(cancellationToken);

        // Send OTP via templated notification
        await notificationService.SendTemplatedNotificationAsync(
            user.Id,
            "PasswordReset",
            new Dictionary<string, string> { { "Otp", otp } },
            "AppUser",
            user.Id.ToString(),
            cancellationToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var mail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == mail, cancellationToken);
        if (user is null) return false;

        if (string.IsNullOrWhiteSpace(user.PasswordResetOtpHash) || 
            !user.PasswordResetOtpExpiresAtUtc.HasValue || 
            user.PasswordResetOtpExpiresAtUtc.Value < DateTime.UtcNow)
        {
            return false;
        }

        var inputHash = Hash(token.Trim());
        if (user.PasswordResetOtpHash != inputHash)
        {
            return false;
        }

        // Reset password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetOtpHash = null;
        user.PasswordResetOtpExpiresAtUtc = null;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await notificationService.SendManualNotificationAsync(new SendManualNotificationRequest(
            user.Id,
            "Password Changed",
            "Your account password was recently changed. If this wasn't you, contact support immediately.",
            "Warning",
            "InApp",
            "High"
        ), cancellationToken);

        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await notificationService.SendManualNotificationAsync(new SendManualNotificationRequest(
            user.Id,
            "Password Changed",
            "Your account password was recently changed. If this wasn't you, contact support immediately.",
            "Warning",
            "InApp",
            "High"
        ), cancellationToken);

        return true;
    }

    private async Task<AuthResponse> IssueTokensAsync(AppUser user, bool rememberMe, CancellationToken cancellationToken)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var jwt = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expires, signingCredentials: credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshTokenDays = rememberMe ? 30 : _jwt.RefreshTokenDays;

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays)
        });
        await db.SaveChangesAsync(cancellationToken);

        // Set refresh token in HTTP-only Secure cookie
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Must be true for SameSite = None
                SameSite = SameSiteMode.None, // Crucial for cross-origin local testing (localhost:4200 calling localhost:7002)
                Expires = DateTime.UtcNow.AddDays(refreshTokenDays),
                Path = "/"
            };
            httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(), accessToken, refreshToken, expires);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

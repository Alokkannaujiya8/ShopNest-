using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Auth;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<bool>>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterCommand(request), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Registration successful. Please verify the OTP sent to your email."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Login successful."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Token refreshed successfully."));
    }

    [HttpPost("verify-email-otp")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyEmailOtp(VerifyEmailOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new VerifyEmailOtpCommand(request.Email, request.Otp), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Email verified successfully."));
    }

    [HttpPost("resend-email-otp")]
    public async Task<ActionResult<ApiResponse<bool>>> ResendEmailOtp(ResendEmailOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResendEmailOtpCommand(request.Email), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Verification OTP has been resent to your email."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "If the email exists, an OTP has been sent."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResetPasswordCommand(request.Email, request.Otp, request.NewPassword), cancellationToken);
        if (!result)
        {
            return BadRequest(ApiResponse.FailureResult("Invalid or expired OTP."));
        }
        return Ok(ApiResponse<bool>.SuccessResult(true, "Password has been successfully reset."));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions 
        { 
            Path = "/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        return Ok(ApiResponse.SuccessResult("Logged out successfully."));
    }
}

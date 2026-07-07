using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Auth;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<bool>;
public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AuthResponse>;
public sealed record ForgotPasswordCommand(string Email) : IRequest<bool>;
public sealed record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest<bool>;
public sealed record VerifyEmailOtpCommand(string Email, string Otp) : IRequest<AuthResponse>;
public sealed record ResendEmailOtpCommand(string Email) : IRequest<bool>;
public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

public sealed class AuthHandlers(IAuthService auth) :
    IRequestHandler<RegisterCommand, bool>,
    IRequestHandler<LoginCommand, AuthResponse>,
    IRequestHandler<RefreshTokenCommand, AuthResponse>,
    IRequestHandler<ForgotPasswordCommand, bool>,
    IRequestHandler<ResetPasswordCommand, bool>,
    IRequestHandler<VerifyEmailOtpCommand, AuthResponse>,
    IRequestHandler<ResendEmailOtpCommand, bool>,
    IRequestHandler<ChangePasswordCommand, bool>
{
    public Task<bool> Handle(RegisterCommand command, CancellationToken cancellationToken) =>
        auth.RegisterAsync(command.Request, cancellationToken);

    public Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken) =>
        auth.LoginAsync(command.Request, cancellationToken);

    public Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken) =>
        auth.RefreshAsync(command.Request, cancellationToken);

    public Task<bool> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken) =>
        auth.ForgotPasswordAsync(command.Email, cancellationToken);

    public Task<bool> Handle(ResetPasswordCommand command, CancellationToken cancellationToken) =>
        auth.ResetPasswordAsync(command.Email, command.Otp, command.NewPassword, cancellationToken);

    public Task<AuthResponse> Handle(VerifyEmailOtpCommand command, CancellationToken cancellationToken) =>
        auth.VerifyEmailOtpAsync(command.Email, command.Otp, cancellationToken);

    public Task<bool> Handle(ResendEmailOtpCommand command, CancellationToken cancellationToken) =>
        auth.ResendEmailOtpAsync(command.Email, cancellationToken);

    public Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken) =>
        auth.ChangePasswordAsync(command.UserId, command.CurrentPassword, command.NewPassword, cancellationToken);
}

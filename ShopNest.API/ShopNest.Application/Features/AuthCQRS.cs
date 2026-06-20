using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Auth;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AuthResponse>;
public sealed record ForgotPasswordCommand(string Email) : IRequest<bool>;
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<bool>;
public sealed record VerifyEmailCommand(string Email, string Token) : IRequest<bool>;
public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

public sealed class AuthHandlers(IAuthService auth) :
    IRequestHandler<RegisterCommand, AuthResponse>,
    IRequestHandler<LoginCommand, AuthResponse>,
    IRequestHandler<RefreshTokenCommand, AuthResponse>,
    IRequestHandler<ForgotPasswordCommand, bool>,
    IRequestHandler<ResetPasswordCommand, bool>,
    IRequestHandler<VerifyEmailCommand, bool>,
    IRequestHandler<ChangePasswordCommand, bool>
{
    public Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken) =>
        auth.RegisterAsync(command.Request, cancellationToken);

    public Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken) =>
        auth.LoginAsync(command.Request, cancellationToken);

    public Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken) =>
        auth.RefreshAsync(command.Request, cancellationToken);

    public Task<bool> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken) =>
        auth.ForgotPasswordAsync(command.Email, cancellationToken);

    public Task<bool> Handle(ResetPasswordCommand command, CancellationToken cancellationToken) =>
        auth.ResetPasswordAsync(command.Email, command.Token, command.NewPassword, cancellationToken);

    public Task<bool> Handle(VerifyEmailCommand command, CancellationToken cancellationToken) =>
        auth.VerifyEmailAsync(command.Email, command.Token, cancellationToken);

    public Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken) =>
        auth.ChangePasswordAsync(command.UserId, command.CurrentPassword, command.NewPassword, cancellationToken);
}

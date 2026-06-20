using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Auth;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public Task<AuthResponse> Register(RegisterRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new RegisterCommand(request), cancellationToken);

    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new LoginCommand(request), cancellationToken);

    [HttpPost("refresh")]
    public Task<AuthResponse> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new RefreshTokenCommand(request), cancellationToken);
}

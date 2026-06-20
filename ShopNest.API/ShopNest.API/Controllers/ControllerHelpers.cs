using System.Security.Claims;

namespace ShopNest.API.Controllers;

internal static class ControllerHelpers
{
    public static Guid UserId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User id claim missing."));
}

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class WishlistController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var guid))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return guid;
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<WishlistItemDto>>> GetWishlist(
        [FromQuery] WishlistSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetWishlistQuery(CurrentUserId, request), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetWishlistCount(CancellationToken cancellationToken)
    {
        try
        {
            var count = await mediator.Send(new GetWishlistCountQuery(CurrentUserId), cancellationToken);
            return Ok(count);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("add/{productId:guid}")]
    public async Task<ActionResult<WishlistItemDto>> AddToWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new AddToWishlistCommand(CurrentUserId, productId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("remove/{productId:guid}")]
    public async Task<ActionResult<bool>> RemoveFromWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new RemoveFromWishlistCommand(CurrentUserId, productId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<bool>> ClearWishlist(CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ClearWishlistCommand(CurrentUserId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("move-to-cart/{productId:guid}")]
    public async Task<ActionResult<bool>> MoveToCart(
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new MoveToCartCommand(CurrentUserId, productId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}

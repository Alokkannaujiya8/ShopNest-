using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;
using ShopNest.Application.Features.Profile;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CheckoutController(IMediator mediator) : ControllerBase
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

    [HttpGet("summary")]
    public async Task<ActionResult<CheckoutSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await mediator.Send(new GetCheckoutSummaryQuery(CurrentUserId), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("shipping-methods")]
    public async Task<ActionResult<IReadOnlyList<ShippingMethodDto>>> GetShippingMethods(CancellationToken cancellationToken)
    {
        var summary = await mediator.Send(new GetCheckoutSummaryQuery(CurrentUserId), cancellationToken);
        return Ok(summary.ShippingMethods);
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetPaymentMethods(CancellationToken cancellationToken)
    {
        var summary = await mediator.Send(new GetCheckoutSummaryQuery(CurrentUserId), cancellationToken);
        return Ok(summary.PaymentMethods);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<CheckoutValidationResult>> ValidateCheckout(
        [FromBody] CheckoutValidationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ValidateCheckoutCommand(CurrentUserId, request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("place-order")]
    public async Task<ActionResult<OrderDto>> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await mediator.Send(new PlaceOrderCommand(CurrentUserId, request), cancellationToken);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Address shortcuts for checkout screen
    [HttpPost("addresses")]
    public async Task<ActionResult<UserAddressDto>> AddAddress(
        [FromBody] AddAddressRequest request,
        CancellationToken cancellationToken)
    {
        var address = await mediator.Send(new AddAddressCommand(CurrentUserId, request), cancellationToken);
        return Ok(address);
    }

    [HttpPut("addresses/{id:guid}")]
    public async Task<ActionResult<UserAddressDto>> UpdateAddress(
        Guid id,
        [FromBody] AddAddressRequest request,
        CancellationToken cancellationToken)
    {
        var address = await mediator.Send(new UpdateAddressCommand(CurrentUserId, id, request), cancellationToken);
        if (address == null) return NotFound("Address not found.");
        return Ok(address);
    }

    [HttpDelete("addresses/{id:guid}")]
    public async Task<ActionResult<bool>> DeleteAddress(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAddressCommand(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("addresses/{id:guid}/default")]
    public async Task<ActionResult<bool>> SetDefaultAddress(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetDefaultAddressCommand(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }
}

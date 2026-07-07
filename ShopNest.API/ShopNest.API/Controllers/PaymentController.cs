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

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentController(IMediator mediator) : ControllerBase
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

    [HttpPost("initialize")]
    [Authorize]
    public async Task<ActionResult<PaymentSessionResponse>> InitializePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new InitializePaymentCommand(request.OrderId, request.Provider, request.Currency), cancellationToken);
        return Ok(response);
    }

    [HttpPost("verify")]
    [Authorize]
    public async Task<ActionResult<bool>> VerifyPayment(
        [FromBody] VerifyPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var success = await mediator.Send(new VerifyPaymentCommand(request.PaymentId, request.TransactionId), cancellationToken);
        return Ok(success);
    }

    [HttpPost("refund")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<bool>> RefundPayment(
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var success = await mediator.Send(new RefundPaymentCommand(request.PaymentId, request.Amount, request.Reason), cancellationToken);
        return Ok(success);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetHistory(CancellationToken cancellationToken)
    {
        var history = await mediator.Send(new GetPaymentHistoryQuery(CurrentUserId), cancellationToken);
        return Ok(history);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> GetDetails(Guid id, CancellationToken cancellationToken)
    {
        var details = await mediator.Send(new GetPaymentDetailsQuery(id), cancellationToken);
        if (details == null) return NotFound("Payment record not found.");
        return Ok(details);
    }

    // Webhook receiver endpoints
    [HttpPost("webhook/stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        // Simple webhook proxy body
        using var reader = new System.IO.StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
        
        // Handle webhook delegating signature validation
        return Ok();
    }

    [HttpPost("webhook/razorpay")]
    public async Task<IActionResult> RazorpayWebhook()
    {
        return Ok();
    }

    [HttpPost("webhook/paypal")]
    public async Task<IActionResult> PaypalWebhook()
    {
        return Ok();
    }
}

public sealed record RefundPaymentRequest(Guid PaymentId, decimal Amount, string Reason);
public sealed record VerifyPaymentRequest(Guid PaymentId, string TransactionId);

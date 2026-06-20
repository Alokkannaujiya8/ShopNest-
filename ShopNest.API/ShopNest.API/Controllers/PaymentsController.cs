using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Payments;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public Task<PaymentSessionResponse> Create(CreatePaymentRequest request, CancellationToken cancellationToken) =>
        mediator.Send(new CreatePaymentSessionCommand(request), cancellationToken);

    [HttpPost("webhooks/{provider}")]
    public async Task<IActionResult> Webhook(string provider, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        await mediator.Send(new HandlePaymentWebhookCommand(provider, payload, signature), cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(CompletePaymentRequest request, CancellationToken cancellationToken) =>
        await mediator.Send(new CompletePaymentSimulationCommand(request.PaymentId, request.Status), cancellationToken) ? NoContent() : NotFound();
}

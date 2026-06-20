using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Enums;

namespace ShopNest.Application.Features.Payments;

public sealed record CreatePaymentSessionCommand(CreatePaymentRequest Request) : IRequest<PaymentSessionResponse>;
public sealed record CompletePaymentSimulationCommand(Guid PaymentId, PaymentStatus Status) : IRequest<bool>;
public sealed record HandlePaymentWebhookCommand(string Provider, string Payload, string? Signature) : IRequest<bool>;

public sealed class PaymentHandlers(IPaymentService payments) :
    IRequestHandler<CreatePaymentSessionCommand, PaymentSessionResponse>,
    IRequestHandler<CompletePaymentSimulationCommand, bool>,
    IRequestHandler<HandlePaymentWebhookCommand, bool>
{
    public Task<PaymentSessionResponse> Handle(CreatePaymentSessionCommand command, CancellationToken cancellationToken) =>
        payments.CreatePaymentAsync(command.Request, cancellationToken);

    public Task<bool> Handle(CompletePaymentSimulationCommand command, CancellationToken cancellationToken) =>
        payments.CompletePaymentAsync(new CompletePaymentRequest(command.PaymentId, command.Status), cancellationToken);

    public Task<bool> Handle(HandlePaymentWebhookCommand command, CancellationToken cancellationToken) =>
        payments.HandleWebhookAsync(command.Provider, command.Payload, command.Signature, cancellationToken).ContinueWith(_ => true);
}

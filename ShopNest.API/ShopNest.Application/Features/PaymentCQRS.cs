using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

public sealed record InitializePaymentCommand(Guid OrderId, string Provider, string Currency) : IRequest<PaymentSessionResponse>;
public sealed record VerifyPaymentCommand(Guid PaymentId, string TransactionId) : IRequest<bool>;
public sealed record RefundPaymentCommand(Guid PaymentId, decimal Amount, string Reason) : IRequest<bool>;
public sealed record GetPaymentHistoryQuery(Guid UserId) : IRequest<IReadOnlyList<PaymentDto>>;
public sealed record GetPaymentDetailsQuery(Guid PaymentId) : IRequest<PaymentDto?>;

public sealed class PaymentCQRSHandlers(IPaymentService paymentService) :
    IRequestHandler<InitializePaymentCommand, PaymentSessionResponse>,
    IRequestHandler<VerifyPaymentCommand, bool>,
    IRequestHandler<RefundPaymentCommand, bool>,
    IRequestHandler<GetPaymentHistoryQuery, IReadOnlyList<PaymentDto>>,
    IRequestHandler<GetPaymentDetailsQuery, PaymentDto?>
{
    public Task<PaymentSessionResponse> Handle(InitializePaymentCommand request, CancellationToken cancellationToken)
    {
        return paymentService.InitializePaymentAsync(request.OrderId, request.Provider, request.Currency, cancellationToken);
    }

    public Task<bool> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
    {
        return paymentService.VerifyPaymentAsync(request.PaymentId, request.TransactionId, cancellationToken);
    }

    public Task<bool> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        return paymentService.RefundPaymentAsync(request.PaymentId, request.Amount, request.Reason, cancellationToken);
    }

    public Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        return paymentService.GetPaymentHistoryAsync(request.UserId, cancellationToken);
    }

    public Task<PaymentDto?> Handle(GetPaymentDetailsQuery request, CancellationToken cancellationToken)
    {
        return paymentService.GetPaymentDetailsAsync(request.PaymentId, cancellationToken);
    }
}

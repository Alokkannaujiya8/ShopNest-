using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;

namespace ShopNest.Application.Features;

public sealed record ShippingMethodDto(string Code, string Name, string Description, decimal Cost, int EstimatedDays);

public sealed record CheckoutSummaryDto(
    CartDto Cart,
    IReadOnlyList<UserAddressDto> Addresses,
    IReadOnlyList<ShippingMethodDto> ShippingMethods,
    IReadOnlyList<string> PaymentMethods
);

public sealed record CheckoutValidationRequest(
    Guid ShippingAddressId,
    string ShippingMethodCode,
    string PaymentMethod
);

public sealed record CheckoutValidationResult(
    bool IsValid,
    string? ErrorMessage,
    decimal Subtotal,
    decimal Discount,
    decimal ShippingCost,
    decimal Tax,
    decimal GrandTotal
);

public sealed record PlaceOrderRequest(
    Guid ShippingAddressId,
    string ShippingMethodCode,
    string PaymentMethod,
    string? OrderNotes,
    string? DeliveryInstructions
);

public sealed record GetCheckoutSummaryQuery(Guid UserId) : IRequest<CheckoutSummaryDto>;
public sealed record ValidateCheckoutCommand(Guid UserId, CheckoutValidationRequest Request) : IRequest<CheckoutValidationResult>;
public sealed record PlaceOrderCommand(Guid UserId, PlaceOrderRequest Request) : IRequest<OrderDto>;

public sealed class CheckoutCQRSHandlers(
    IRepository<Product> productRepository,
    IRepository<UserAddress> addressRepository,
    ICartOrderService cartOrderService,
    IUserProfileService userProfileService
) :
    IRequestHandler<GetCheckoutSummaryQuery, CheckoutSummaryDto>,
    IRequestHandler<ValidateCheckoutCommand, CheckoutValidationResult>,
    IRequestHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<CheckoutSummaryDto> Handle(GetCheckoutSummaryQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartOrderService.GetCartAsync(request.UserId, cancellationToken);
        var addresses = await userProfileService.GetAddressesAsync(request.UserId, cancellationToken);

        var subtotal = cart.Subtotal;
        var shippingMethods = new List<ShippingMethodDto>
        {
            new("Standard", "Standard Shipping", "Delivery in 5-7 business days", subtotal >= 100m ? 0 : 10m, 7),
            new("Express", "Express Shipping", "Delivery in 1-2 business days", 25m, 2)
        };

        var paymentMethods = new List<string> { "Stripe", "CashOnDelivery" };

        return new CheckoutSummaryDto(cart, addresses, shippingMethods, paymentMethods);
    }

    public async Task<CheckoutValidationResult> Handle(ValidateCheckoutCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartOrderService.GetCartAsync(command.UserId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return new CheckoutValidationResult(false, "Cart is empty.", 0, 0, 0, 0, 0);
        }

        // Validate stock
        foreach (var item in cart.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null || product.IsDeleted || !product.IsActive || !product.IsPublished)
            {
                return new CheckoutValidationResult(false, $"Product '{item.ProductName}' is no longer available.", 0, 0, 0, 0, 0);
            }
            if (product.StockQuantity < item.Quantity)
            {
                return new CheckoutValidationResult(false, $"Insufficient stock for product '{item.ProductName}'. Only {product.StockQuantity} units available.", 0, 0, 0, 0, 0);
            }
        }

        // Validate shipping address
        var addresses = await addressRepository.FindAsync(x => x.UserId == command.UserId && x.Id == command.Request.ShippingAddressId, cancellationToken);
        if (!addresses.Any())
        {
            return new CheckoutValidationResult(false, "Invalid shipping address.", 0, 0, 0, 0, 0);
        }

        // Calculate pricing
        var subtotal = cart.Subtotal;
        var discount = cart.CouponDiscount;
        decimal shippingCost = command.Request.ShippingMethodCode.ToLower() switch
        {
            "express" => 25m,
            "standard" => subtotal >= 100m ? 0m : 10m,
            _ => 0m
        };

        var tax = (subtotal - discount) * 0.10m;
        if (tax < 0) tax = 0;
        var grandTotal = subtotal - discount + shippingCost + tax;

        return new CheckoutValidationResult(true, null, subtotal, discount, shippingCost, tax, grandTotal);
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        // First validate
        var validation = await Handle(new ValidateCheckoutCommand(command.UserId, new CheckoutValidationRequest(
            command.Request.ShippingAddressId,
            command.Request.ShippingMethodCode,
            command.Request.PaymentMethod
        )), cancellationToken);

        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Checkout validation failed.");
        }

        // Fetch address details
        var address = await addressRepository.GetByIdAsync(command.Request.ShippingAddressId, cancellationToken)
            ?? throw new InvalidOperationException("Selected shipping address not found.");

        var fullAddress = $"{address.FullName}\n" +
                          $"{address.AddressLine1}, {address.AddressLine2}\n" +
                          $"{address.Area}, {address.City}, {address.State} - {address.PostalCode}\n" +
                          $"Mobile: {address.MobileNumber} | Alternate: {address.AlternateMobile}\n" +
                          $"Instructions: {command.Request.DeliveryInstructions}\n" +
                          $"Notes: {command.Request.OrderNotes}";

        // Place order
        return await cartOrderService.CheckoutAsync(
            command.UserId, 
            new CheckoutRequest(
                fullAddress, 
                command.Request.PaymentMethod, 
                "USD",
                fullAddress,
                command.Request.OrderNotes,
                command.Request.DeliveryInstructions
            ), 
            cancellationToken);
    }
}

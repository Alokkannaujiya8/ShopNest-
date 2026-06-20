using ShopNest.Application.Dtos;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Services;

internal static class MappingExtensions
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Name,
        product.Slug,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.IsActive,
        new CategoryDto(product.Category.Id, product.Category.Name, product.Category.Slug),
        product.Images.Select(x => new ProductImageDto(x.Id, x.Url, x.IsPrimary)).ToList());

    public static CartDto ToDto(this Cart cart)
    {
        var items = cart.Items.Select(x => new CartItemDto(
            x.Id,
            x.ProductId,
            x.Product.Name,
            x.Product.Price,
            x.Quantity,
            x.Product.Price * x.Quantity)).ToList();

        return new CartDto(cart.Id, items, items.Sum(x => x.LineTotal));
    }

    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.Status,
        order.TotalAmount,
        order.ShippingAddress,
        order.Items.Select(x => new OrderItemDto(x.ProductId, x.ProductName, x.UnitPrice, x.Quantity, x.UnitPrice * x.Quantity)).ToList(),
        order.Payment is null ? null : new PaymentDto(order.Payment.Id, order.Payment.Provider, order.Payment.ProviderPaymentId, order.Payment.ProviderOrderId, order.Payment.Status, order.Payment.Amount, order.Payment.Currency));

    public static string Slugify(string value)
    {
        var slug = new string(value.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return slug.Trim('-');
    }
}

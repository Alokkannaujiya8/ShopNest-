using ShopNest.Application.Dtos;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Services;

internal static class MappingExtensions
{
    public static ProductAttributeValueDto ToDto(this ProductAttributeValue val) => new(
        val.Id,
        val.ProductAttributeId,
        val.ProductAttribute != null ? val.ProductAttribute.Name : string.Empty,
        val.Value);

    public static ProductVariantDto ToDto(this ProductVariant variant) => new(
        variant.Id,
        variant.Name,
        variant.Sku,
        variant.Barcode,
        variant.Price,
        variant.StockQuantity,
        variant.ImageUrl,
        variant.IsActive,
        variant.AttributeValues != null ? variant.AttributeValues.Select(x => x.ToDto()).ToList() : new List<ProductAttributeValueDto>());

    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Name,
        product.Sku,
        product.Barcode,
        product.Slug,
        product.ShortDescription,
        product.Description,
        product.CategoryId,
        product.Category != null ? new CategoryDto(product.Category.Id, product.Category.Name, product.Category.Slug) : null!,
        product.SubCategoryId,
        product.SubCategory != null ? new CategoryDto(product.SubCategory.Id, product.SubCategory.Name, product.SubCategory.Slug) : null,
        product.BrandId,
        product.Brand?.Name,
        product.CostPrice,
        product.Price,
        product.DiscountType,
        product.DiscountValue,
        product.DiscountStartDate,
        product.DiscountEndDate,
        product.TaxPercentage,
        product.StockQuantity,
        product.MinimumStock,
        product.MaximumStock,
        product.StockStatus,
        product.Weight,
        product.Length,
        product.Width,
        product.Height,
        product.MetaTitle,
        product.MetaDescription,
        product.MetaKeywords,
        product.IsFeatured,
        product.IsNewArrival,
        product.IsBestSeller,
        product.IsActive,
        product.IsPublished,
        product.Reviews != null && product.Reviews.Any(r => r.IsApproved) ? product.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0.0,
        product.Reviews != null ? product.Reviews.Count(r => r.IsApproved) : 0,
        product.Images != null ? product.Images.OrderBy(x => x.DisplayOrder).Select(x => new ProductImageDto(x.Id, x.Url, x.IsPrimary, x.DisplayOrder)).ToList() : new List<ProductImageDto>(),
        product.Variants != null ? product.Variants.Select(x => x.ToDto()).ToList() : new List<ProductVariantDto>());

    public static CartDto ToDto(this Cart cart, Coupon? coupon = null)
    {
        var items = cart.Items.Select(x => {
            var p = x.Product;
            var primaryImg = p.Images?.FirstOrDefault(i => i.IsPrimary)?.Url ?? p.Images?.FirstOrDefault()?.Url;
            return new CartItemDto(
                x.Id,
                x.ProductId,
                p.Name,
                p.Sku,
                p.Brand?.Name,
                p.Category?.Name ?? string.Empty,
                primaryImg,
                p.Price,
                p.Price + p.DiscountValue,
                p.Price,
                x.Quantity,
                p.Price * x.Quantity,
                p.StockQuantity > 0 ? "InStock" : "OutOfStock",
                p.StockQuantity
            );
        }).ToList();

        var subtotal = items.Sum(x => x.LineTotal);
        var totalDiscount = cart.Items.Sum(x => x.Product.DiscountValue * x.Quantity);

        decimal couponDiscount = 0;
        if (coupon != null && coupon.IsActive && coupon.ExpiresAtUtc > DateTime.UtcNow && subtotal >= coupon.MinOrderAmount)
        {
            if (string.Equals(coupon.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
            {
                couponDiscount = subtotal * (coupon.DiscountValue / 100);
            }
            else if (string.Equals(coupon.DiscountType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
            {
                couponDiscount = coupon.DiscountValue;
            }

            if (coupon.MaxDiscountAmount.HasValue && couponDiscount > coupon.MaxDiscountAmount.Value)
            {
                couponDiscount = coupon.MaxDiscountAmount.Value;
            }
        }

        var shippingCharges = (subtotal > 0 && subtotal < 100) ? 10m : 0m;
        var estimatedTax = (subtotal - couponDiscount) * 0.10m;
        if (estimatedTax < 0) estimatedTax = 0;
        var grandTotal = subtotal - couponDiscount + shippingCharges + estimatedTax;

        return new CartDto(
            cart.Id,
            items,
            subtotal,
            totalDiscount,
            cart.AppliedCouponCode,
            couponDiscount,
            shippingCharges,
            estimatedTax,
            grandTotal
        );
    }

    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.Status,
        order.TotalAmount,
        order.ShippingAddress,
        order.Items.Select(x => new OrderItemDto(
            x.ProductId, 
            x.ProductName, 
            x.UnitPrice, 
            x.Quantity, 
            x.Total == 0 ? x.UnitPrice * x.Quantity : x.Total,
            x.ProductVariantId,
            x.Sku,
            x.Discount,
            x.Tax,
            x.Total == 0 ? x.UnitPrice * x.Quantity : x.Total)).ToList(),
        order.Payment is null ? null : new PaymentDto(order.Payment.Id, order.Payment.Provider, order.Payment.ProviderPaymentId, order.Payment.ProviderOrderId, order.Payment.Status, order.Payment.Amount, order.Payment.Currency),
        order.BillingAddress,
        order.PaymentMethod,
        order.CourierPartner,
        order.TrackingNumber,
        order.ShippingCost,
        order.Tax,
        order.Discount,
        order.OrderNotes,
        order.EstimatedDeliveryDate,
        order.DeliveredDate);

    public static OrderStatusHistoryDto ToDto(this OrderStatusHistory history) => new(
        history.Id,
        history.Status,
        history.Note,
        history.ChangedBy,
        history.CreatedAtUtc);

    public static OrderTrackingDto ToDto(this OrderTracking tracking) => new(
        tracking.Id,
        tracking.CourierPartner,
        tracking.TrackingNumber,
        tracking.Status,
        tracking.Location,
        tracking.CreatedAtUtc);

    public static string Slugify(string value)
    {
        var slug = new string(value.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return slug.Trim('-');
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class AdvancedFeaturesService : IAdvancedFeaturesService
{
    private readonly ShopNestDbContext _dbContext;
    private readonly IMapper _mapper;

    public AdvancedFeaturesService(ShopNestDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GlobalSearchResultDto> GlobalSearchAsync(string queryText, int limit, CancellationToken cancellationToken)
    {
        var query = queryText?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(query))
        {
            return new GlobalSearchResultDto([], [], [], []);
        }

        // 1. Search Products
        var products = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
            .Where(p => p.IsActive && p.IsPublished && !p.IsDeleted &&
                        (p.Name.ToLower().Contains(query) || 
                         p.Sku.ToLower().Contains(query) || 
                         p.Description.ToLower().Contains(query) ||
                         (p.Brand != null && p.Brand.Name.ToLower().Contains(query))))
            .Take(limit)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // 2. Search Categories
        var categories = await _dbContext.Categories
            .Where(c => c.IsActive && !c.IsDeleted && c.Name.ToLower().Contains(query))
            .Take(limit)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // 3. Search Brands
        var brands = await _dbContext.Brands
            .Where(b => !b.IsDeleted && b.Name.ToLower().Contains(query))
            .Select(b => b.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);

        // 4. Search Coupons (Active)
        var coupons = await _dbContext.Coupons
            .Where(c => c.IsActive && !c.IsDeleted && c.Code.ToLower().Contains(query) && c.ExpiresAtUtc > DateTime.UtcNow)
            .Select(c => c.Code)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new GlobalSearchResultDto(products, categories, brands, coupons);
    }

    public async Task<List<ProductCompareResultDto>> CompareProductsAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds == null || productIds.Count == 0)
        {
            return [];
        }

        var ids = productIds.Take(4).ToList();

        var products = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
            .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return products.Select(p => new ProductCompareResultDto(
            p.Id,
            p.Name,
            p.Sku,
            p.Price,
            p.Brand?.Name ?? "N/A",
            p.Category?.Name ?? "N/A",
            p.Reviews.Any(r => r.IsApproved) ? p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0.0,
            p.Reviews.Count(r => r.IsApproved),
            p.StockStatus,
            p.Weight,
            $"{p.Length} x {p.Width} x {p.Height} cm",
            p.Variants.Select(v => v.Name).ToList()
        )).ToList();
    }

    public async Task<List<TimelineEventDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.StatusHistory)
            .Include(o => o.TrackingUpdates)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null) return [];

        var timeline = new List<TimelineEventDto>();

        // Order Placed (Always completed)
        timeline.Add(new TimelineEventDto(
            "Order Placed",
            $"Order #{order.OrderNumber} was successfully generated.",
            order.CreatedAtUtc,
            true
        ));

        // Payment status
        if (order.Payment != null)
        {
            timeline.Add(new TimelineEventDto(
                "Payment Settled",
                $"Paid via {order.PaymentMethod ?? "System Card"}. Transaction ID: {order.Payment.ProviderPaymentId ?? "N/A"}",
                order.Payment.CreatedAtUtc,
                order.Payment.Status == PaymentStatus.Succeeded
            ));
        }
        else
        {
            timeline.Add(new TimelineEventDto(
                "Payment Pending",
                "Awaiting payment clearance verification.",
                order.CreatedAtUtc,
                false
            ));
        }

        // Status history entries
        foreach (var history in order.StatusHistory.OrderBy(h => h.CreatedAtUtc))
        {
            timeline.Add(new TimelineEventDto(
                $"Status updated to {history.Status}",
                history.Note ?? $"Status set to {history.Status}.",
                history.CreatedAtUtc,
                true
            ));
        }

        // Shipping Tracking updates
        foreach (var tracking in order.TrackingUpdates.OrderBy(t => t.CreatedAtUtc))
        {
            timeline.Add(new TimelineEventDto(
                $"Package Status: {tracking.Status}",
                $"Package status: {tracking.Status} at {tracking.Location} (Courier: {tracking.CourierPartner}, Tracking ID: {tracking.TrackingNumber})",
                tracking.CreatedAtUtc,
                true
            ));
        }

        return timeline.OrderBy(e => e.Timestamp).ToList();
    }

    public async Task<bool> RestoreRecordAsync(string entityName, Guid id, CancellationToken cancellationToken)
    {
        switch (entityName.ToLower())
        {
            case "product":
                var product = await _dbContext.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                if (product == null) return false;
                product.IsDeleted = false;
                product.DeletedAtUtc = null;
                break;

            case "category":
                var category = await _dbContext.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (category == null) return false;
                category.IsDeleted = false;
                category.DeletedAtUtc = null;
                break;

            case "brand":
                var brand = await _dbContext.Brands.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
                if (brand == null) return false;
                brand.IsDeleted = false;
                brand.DeletedAtUtc = null;
                break;

            case "coupon":
                var coupon = await _dbContext.Coupons.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (coupon == null) return false;
                coupon.IsDeleted = false;
                coupon.DeletedAtUtc = null;
                break;

            case "review":
                var review = await _dbContext.Reviews.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
                if (review == null) return false;
                review.IsDeleted = false;
                review.DeletedAtUtc = null;
                break;

            default:
                return false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

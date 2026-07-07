using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class WishlistService(
    ShopNestDbContext db,
    ICartOrderService cartOrderService
) : IWishlistService
{
    public async Task<PagedResult<WishlistItemDto>> GetWishlistAsync(
        Guid userId,
        WishlistSearchRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.WishlistItems
            .Include(x => x.Product)
            .ThenInclude(p => p.Category)
            .Include(x => x.Product)
            .ThenInclude(p => p.Brand)
            .Include(x => x.Product)
            .ThenInclude(p => p.Images)
            .Include(x => x.Product)
            .ThenInclude(p => p.Reviews)
            .Where(x => x.UserId == userId)
            .AsNoTracking();

        // Filters
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var q = request.Query.Trim().ToLower();
            query = query.Where(x =>
                x.Product.Name.ToLower().Contains(q) ||
                x.Product.Sku.ToLower().Contains(q) ||
                (x.Product.Brand != null && x.Product.Brand.Name.ToLower().Contains(q)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Product.CategoryId == request.CategoryId.Value);
        }

        if (request.BrandId.HasValue)
        {
            query = query.Where(x => x.Product.BrandId == request.BrandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.StockStatus))
        {
            query = request.StockStatus.ToLower() switch
            {
                "instock" => query.Where(x => x.Product.StockQuantity > 0),
                "outofstock" => query.Where(x => x.Product.StockQuantity == 0),
                _ => query
            };
        }

        if (request.IsDiscounted == true)
        {
            query = query.Where(x => x.Product.DiscountValue > 0);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(x => x.Product.Name)
                : query.OrderBy(x => x.Product.Name),
            "price" => request.SortDescending
                ? query.OrderByDescending(x => x.Product.Price)
                : query.OrderBy(x => x.Product.Price),
            "dateadded" or "recentlyadded" => request.SortDescending
                ? query.OrderByDescending(x => x.CreatedAtUtc)
                : query.OrderBy(x => x.CreatedAtUtc),
            _ => request.SortDescending
                ? query.OrderByDescending(x => x.CreatedAtUtc)
                : query.OrderBy(x => x.CreatedAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var mapped = items.Select(MapToDto).ToList();
        return new PagedResult<WishlistItemDto>(mapped, page, pageSize, totalCount);
    }

    public async Task<int> GetWishlistCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.WishlistItems
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<WishlistItemDto> AddToWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        if (!product.IsActive || !product.IsPublished)
        {
            throw new InvalidOperationException("Only active and published products can be wishlisted.");
        }

        var item = await db.WishlistItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);

        if (item == null)
        {
            item = new WishlistItem
            {
                UserId = userId,
                ProductId = productId
            };
            db.WishlistItems.Add(item);
            await db.SaveChangesAsync(cancellationToken);
        }

        item.Product = product; // Populate to map properly
        return MapToDto(item);
    }

    public async Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var item = await db.WishlistItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);

        if (item == null) return false;

        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ClearWishlistAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await db.WishlistItems
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0) return false;

        db.WishlistItems.RemoveRange(items);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MoveToCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var item = await db.WishlistItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);

        if (item == null) return false;

        // Add to active cart
        await cartOrderService.AddToCartAsync(userId, new AddCartItemRequest(productId, 1), cancellationToken);

        // Delete from wishlist
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static WishlistItemDto MapToDto(WishlistItem x)
    {
        var p = x.Product;
        var primaryImg = p.Images?.FirstOrDefault(i => i.IsPrimary)?.Url ?? p.Images?.FirstOrDefault()?.Url;
        var avgRating = p.Reviews != null && p.Reviews.Any(r => r.IsApproved) ? p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0.0;
        var revCount = p.Reviews != null ? p.Reviews.Count(r => r.IsApproved) : 0;

        return new WishlistItemDto(
            x.Id,
            x.ProductId,
            p.Name,
            p.Sku,
            p.Slug,
            p.Brand?.Name,
            p.Category?.Name ?? string.Empty,
            p.Price,
            p.Price + p.DiscountValue,
            p.DiscountValue,
            p.StockQuantity,
            p.StockStatus,
            avgRating,
            revCount,
            primaryImg,
            x.CreatedAtUtc
        );
    }
}

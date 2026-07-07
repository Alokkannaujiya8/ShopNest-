using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class AiRecommendationService : IAiRecommendationService
{
    private readonly ShopNestDbContext _dbContext;

    public AiRecommendationService(ShopNestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProductDto>> GetRecommendationsForUserAsync(Guid userId, int count = 10)
    {
        // 1. Get user's purchased category IDs
        var userCategoryIds = await _dbContext.OrderItems
            .Include(x => x.Order)
            .Include(x => x.Product)
            .Where(x => x.Order.UserId == userId && !x.Order.IsDeleted)
            .Select(x => x.Product.CategoryId)
            .Distinct()
            .ToListAsync();

        // 2. Fetch active and published products from these categories that the user hasn't purchased yet
        var purchasedProductIds = await _dbContext.OrderItems
            .Include(x => x.Order)
            .Where(x => x.Order.UserId == userId && !x.Order.IsDeleted)
            .Select(x => x.ProductId)
            .Distinct()
            .ToListAsync();

        IQueryable<Product> query = _dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Reviews)
            .Include(x => x.Variants)
            .Where(x => x.IsActive && x.IsPublished && !x.IsDeleted && !purchasedProductIds.Contains(x.Id));

        if (userCategoryIds.Any())
        {
            query = query.Where(x => userCategoryIds.Contains(x.CategoryId));
        }

        var products = await query
            .OrderByDescending(x => x.Reviews.Average(r => (double?)r.Rating) ?? 0.0)
            .Take(count)
            .ToListAsync();

        // Fallback to top-selling active products if recommendations are empty
        if (!products.Any())
        {
            return await GetPopularProductsAsync(count);
        }

        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<ProductDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int count = 5)
    {
        // Find orders containing the target product
        var orderIds = await _dbContext.OrderItems
            .Where(x => x.ProductId == productId)
            .Select(x => x.OrderId)
            .Distinct()
            .ToListAsync();

        if (!orderIds.Any())
        {
            return await GetSimilarProductsAsync(productId, count);
        }

        // Find other products co-occurring in those orders
        var coProductIds = await _dbContext.OrderItems
            .Where(x => orderIds.Contains(x.OrderId) && x.ProductId != productId)
            .GroupBy(x => x.ProductId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(count)
            .ToListAsync();

        var products = await _dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Reviews)
            .Include(x => x.Variants)
            .Where(x => x.IsActive && x.IsPublished && !x.IsDeleted && coProductIds.Contains(x.Id))
            .ToListAsync();

        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<ProductDto>> GetSimilarProductsAsync(Guid productId, int count = 5)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null) return new List<ProductDto>();

        var products = await _dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Reviews)
            .Include(x => x.Variants)
            .Where(x => x.IsActive && x.IsPublished && !x.IsDeleted && x.Id != productId &&
                       (x.CategoryId == product.CategoryId || x.BrandId == product.BrandId))
            .OrderByDescending(x => x.Price)
            .Take(count)
            .ToListAsync();

        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<ProductDto>> GetTrendingProductsAsync(int count = 10)
    {
        // Trending products are defined as products with high sales quantity in the last 30 days
        var startOfLastMonth = DateTime.UtcNow.AddDays(-30);

        var trendingIds = await _dbContext.OrderItems
            .Include(x => x.Order)
            .Where(x => x.Order.CreatedAtUtc >= startOfLastMonth && !x.Order.IsDeleted)
            .GroupBy(x => x.ProductId)
            .OrderByDescending(g => g.Sum(i => i.Quantity))
            .Select(g => g.Key)
            .Take(count)
            .ToListAsync();

        var products = await _dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Reviews)
            .Include(x => x.Variants)
            .Where(x => x.IsActive && x.IsPublished && !x.IsDeleted && trendingIds.Contains(x.Id))
            .ToListAsync();

        // Fallback to top rated if no orders exist yet
        if (!products.Any())
        {
            products = await _dbContext.Products
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Brand)
                .Include(x => x.Images)
                .Include(x => x.Reviews)
                .Include(x => x.Variants)
                .Where(x => x.IsActive && x.IsPublished && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(count)
                .ToListAsync();
        }

        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<ProductDto>> GetPopularProductsAsync(int count = 10)
    {
        var products = await _dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Reviews)
            .Include(x => x.Variants)
            .Where(x => x.IsActive && x.IsPublished && !x.IsDeleted)
            .OrderByDescending(x => x.Reviews.Average(r => (double?)r.Rating) ?? 0.0)
            .ThenByDescending(x => x.Reviews.Count)
            .Take(count)
            .ToListAsync();

        return products.Select(x => x.ToDto()).ToList();
    }
}

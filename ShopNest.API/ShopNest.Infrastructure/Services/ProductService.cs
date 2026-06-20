using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class ProductService(
    ShopNestDbContext db,
    IImageStorageService imageStorage,
    ISearchIndexer searchIndexer,
    IConnectionMultiplexer? redis = null) : IProductService
{
    public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var cacheKey = $"products:{JsonSerializer.Serialize(request)}";

        if (redis is not null)
        {
            try
            {
                var cached = await redis.GetDatabase().StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
                    return JsonSerializer.Deserialize<PagedResult<ProductDto>>(cached!)!;
                }
            }
            catch (Exception)
            {
                // Fall back to database query if Redis connection fails
            }
        }

        var query = db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Images).Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = $"%{request.Query.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, term) || EF.Functions.Like(x.Description, term));
        }

        if (request.CategoryId is not null) query = query.Where(x => x.CategoryId == request.CategoryId);
        if (request.MinPrice is not null) query = query.Where(x => x.Price >= request.MinPrice);
        if (request.MaxPrice is not null) query = query.Where(x => x.Price <= request.MaxPrice);
        if (request.InStock == true) query = query.Where(x => x.StockQuantity > 0);

        var total = await query.CountAsync(cancellationToken);
        var products = await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var result = new PagedResult<ProductDto>(products.Select(x => x.ToDto()).ToList(), page, pageSize, total);

        if (redis is not null)
        {
            try
            {
                await redis.GetDatabase().StringSetAsync(cacheKey, JsonSerializer.Serialize(result), TimeSpan.FromMinutes(5));
            }
            catch (Exception)
            {
                // Fail silently if saving to cache fails
            }
        }

        return result;
    }

    public async Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return product?.ToDto();
    }

    public async Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name?.Trim() ?? string.Empty,
            Slug = MappingExtensions.Slugify(request.Name ?? string.Empty),
            Description = request.Description?.Trim() ?? string.Empty,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        product = await ProductQuery().FirstAsync(x => x.Id == product.Id, cancellationToken);
        await searchIndexer.IndexProductAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.Include(x => x.Category).Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return null;

        product.Name = request.Name?.Trim() ?? string.Empty;
        product.Slug = MappingExtensions.Slugify(request.Name ?? string.Empty);
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        product = await ProductQuery().FirstAsync(x => x.Id == id, cancellationToken);
        await searchIndexer.IndexProductAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        await searchIndexer.DeleteProductAsync(id, cancellationToken);
        return true;
    }

    public async Task<ProductImageDto?> UploadImageAsync(Guid productId, IFormFile file, bool isPrimary, CancellationToken cancellationToken)
    {
        var product = await db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
        if (product is null) return null;

        await using var stream = file.OpenReadStream();
        var upload = await imageStorage.UploadAsync(stream, file.FileName, cancellationToken);
        if (isPrimary)
        {
            foreach (var existing in product.Images) existing.IsPrimary = false;
        }

        var image = new ProductImage { ProductId = productId, Url = upload.Url, PublicId = upload.PublicId, IsPrimary = isPrimary };
        db.ProductImages.Add(image);
        await db.SaveChangesAsync(cancellationToken);
        return new ProductImageDto(image.Id, image.Url, image.IsPrimary);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new CategoryDto(x.Id, x.Name, x.Slug)).ToListAsync(cancellationToken);

    public async Task<CategoryDto> CreateCategoryAsync(UpsertCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category { Name = request.Name.Trim(), Slug = MappingExtensions.Slugify(request.Name) };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Slug);
    }

    private IQueryable<Product> ProductQuery() => db.Products.Include(x => x.Category).Include(x => x.Images);
}

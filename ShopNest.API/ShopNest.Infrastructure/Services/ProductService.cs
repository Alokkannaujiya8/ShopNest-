using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

using System.Security.Claims;

namespace ShopNest.Infrastructure.Services;

public sealed class ProductService(
    ShopNestDbContext db,
    IImageStorageService imageStorage,
    ISearchIndexer searchIndexer,
    IHttpContextAccessor httpContextAccessor,
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

        // Intercept search queries to save to history
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(userIdClaim, out var guid) ? guid : null;
            await SaveSearchQueryAsync(userId, request.Query.Trim(), cancellationToken);
        }

        var query = db.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Reviews)
            .Include(x => x.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
            .AsQueryable();

        // Soft delete filter check
        if (request.IsDeleted == true)
        {
            query = query.IgnoreQueryFilters().Where(x => x.IsDeleted);
        }
        else
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (request.IsActive is not null) query = query.Where(x => x.IsActive == request.IsActive);
        if (request.IsPublished is not null) query = query.Where(x => x.IsPublished == request.IsPublished);
        if (request.IsFeatured is not null) query = query.Where(x => x.IsFeatured == request.IsFeatured);
        if (request.IsNewArrival is not null) query = query.Where(x => x.IsNewArrival == request.IsNewArrival);
        if (request.IsBestSeller is not null) query = query.Where(x => x.IsBestSeller == request.IsBestSeller);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = $"%{request.Query.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, term) || 
                                     EF.Functions.Like(x.Description, term) || 
                                     EF.Functions.Like(x.Sku, term) || 
                                     (x.Barcode != null && EF.Functions.Like(x.Barcode, term)));
        }

        if (request.CategoryId is not null) query = query.Where(x => x.CategoryId == request.CategoryId);
        if (request.SubCategoryId is not null) query = query.Where(x => x.SubCategoryId == request.SubCategoryId);
        if (request.BrandId is not null) query = query.Where(x => x.BrandId == request.BrandId);
        
        if (request.MinPrice is not null) query = query.Where(x => x.Price >= request.MinPrice);
        if (request.MaxPrice is not null) query = query.Where(x => x.Price <= request.MaxPrice);

        if (request.MinRating.HasValue)
        {
            query = query.Where(x => x.Reviews.Any(r => r.IsApproved) && x.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) >= request.MinRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Color))
        {
            var colorVal = request.Color.Trim().ToLower();
            query = query.Where(x => x.Variants.Any(v => v.AttributeValues.Any(av => av.ProductAttribute.Name.ToLower() == "color" && av.Value.ToLower() == colorVal)));
        }

        if (!string.IsNullOrWhiteSpace(request.Size))
        {
            var sizeVal = request.Size.Trim().ToLower();
            query = query.Where(x => x.Variants.Any(v => v.AttributeValues.Any(av => av.ProductAttribute.Name.ToLower() == "size" && av.Value.ToLower() == sizeVal)));
        }

        if (!string.IsNullOrWhiteSpace(request.Material))
        {
            var matVal = request.Material.Trim().ToLower();
            query = query.Where(x => x.Variants.Any(v => v.AttributeValues.Any(av => av.ProductAttribute.Name.ToLower() == "material" && av.Value.ToLower() == matVal)));
        }

        if (request.StockStatus != null)
        {
            query = query.Where(x => x.StockStatus == request.StockStatus);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortDescending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "stock" => request.SortDescending ? query.OrderByDescending(x => x.StockQuantity) : query.OrderBy(x => x.StockQuantity),
            "createddate" or "newest" => request.SortDescending ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            "oldest" => query.OrderBy(x => x.CreatedAtUtc),
            "updateddate" => request.SortDescending ? query.OrderByDescending(x => x.UpdatedAtUtc) : query.OrderBy(x => x.UpdatedAtUtc),
            "bestselling" => query.OrderByDescending(x => x.IsBestSeller).ThenBy(x => x.Name),
            "highestrated" => query.OrderByDescending(x => x.Reviews.Where(r => r.IsApproved).Average(r => (double?)r.Rating) ?? 0.0),
            "mostreviewed" => query.OrderByDescending(x => x.Reviews.Count(r => r.IsApproved)),
            "az" => query.OrderBy(x => x.Name),
            "za" => query.OrderByDescending(x => x.Name),
            _ => request.SortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync(cancellationToken);
        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
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

    public async Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        return product?.ToDto();
    }

    public async Task<IReadOnlyList<ProductDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken)
    {
        var products = await ProductQuery()
            .Where(x => x.IsFeatured && x.IsActive && x.IsPublished)
            .Take(count)
            .ToListAsync(cancellationToken);
        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetBestSellersAsync(int count, CancellationToken cancellationToken)
    {
        var products = await ProductQuery()
            .Where(x => x.IsBestSeller && x.IsActive && x.IsPublished)
            .Take(count)
            .ToListAsync(cancellationToken);
        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetNewArrivalsAsync(int count, CancellationToken cancellationToken)
    {
        var products = await ProductQuery()
            .Where(x => x.IsNewArrival && x.IsActive && x.IsPublished)
            .Take(count)
            .ToListAsync(cancellationToken);
        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var slug = MappingExtensions.Slugify(request.Name);
        
        // Validate uniques
        if (await db.Products.AnyAsync(x => x.Sku == request.Sku, cancellationToken))
            throw new InvalidOperationException($"SKU '{request.Sku}' already exists.");
        if (request.Barcode != null && await db.Products.AnyAsync(x => x.Barcode == request.Barcode, cancellationToken))
            throw new InvalidOperationException($"Barcode '{request.Barcode}' already exists.");
        if (await db.Products.AnyAsync(x => x.Slug == slug, cancellationToken))
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";

        var product = new Product
        {
            Name = request.Name.Trim(),
            Sku = request.Sku.Trim(),
            Barcode = request.Barcode?.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription?.Trim(),
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            BrandId = request.BrandId,
            CostPrice = request.CostPrice,
            Price = request.Price,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            DiscountStartDate = request.DiscountStartDate,
            DiscountEndDate = request.DiscountEndDate,
            TaxPercentage = request.TaxPercentage,
            StockQuantity = request.StockQuantity,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            StockStatus = request.StockStatus,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            MetaKeywords = request.MetaKeywords?.Trim(),
            IsFeatured = request.IsFeatured,
            IsNewArrival = request.IsNewArrival,
            IsBestSeller = request.IsBestSeller,
            IsActive = request.IsActive,
            IsPublished = request.IsPublished
        };

        if (request.Variants != null)
        {
            foreach (var v in request.Variants)
            {
                var variant = new ProductVariant
                {
                    Name = v.Name.Trim(),
                    Sku = v.Sku.Trim(),
                    Barcode = v.Barcode?.Trim(),
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    ImageUrl = v.ImageUrl,
                    IsActive = v.IsActive
                };

                foreach (var av in v.AttributeValues)
                {
                    variant.AttributeValues.Add(new ProductAttributeValue
                    {
                        ProductAttributeId = av.ProductAttributeId,
                        Value = av.Value.Trim()
                    });
                }
                product.Variants.Add(variant);
            }
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        
        product = await ProductQuery().FirstAsync(x => x.Id == product.Id, cancellationToken);
        await searchIndexer.IndexProductAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Include(x => x.Variants)
                .ThenInclude(v => v.AttributeValues)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return null;

        var slug = MappingExtensions.Slugify(request.Name);

        // Validate uniques
        if (await db.Products.AnyAsync(x => x.Sku == request.Sku && x.Id != id, cancellationToken))
            throw new InvalidOperationException($"SKU '{request.Sku}' already exists.");
        if (request.Barcode != null && await db.Products.AnyAsync(x => x.Barcode == request.Barcode && x.Id != id, cancellationToken))
            throw new InvalidOperationException($"Barcode '{request.Barcode}' already exists.");

        product.Name = request.Name.Trim();
        product.Sku = request.Sku.Trim();
        product.Barcode = request.Barcode?.Trim();
        product.ShortDescription = request.ShortDescription?.Trim();
        product.Description = request.Description.Trim();
        product.CategoryId = request.CategoryId;
        product.SubCategoryId = request.SubCategoryId;
        product.BrandId = request.BrandId;
        product.CostPrice = request.CostPrice;
        product.Price = request.Price;
        product.DiscountType = request.DiscountType;
        product.DiscountValue = request.DiscountValue;
        product.DiscountStartDate = request.DiscountStartDate;
        product.DiscountEndDate = request.DiscountEndDate;
        product.TaxPercentage = request.TaxPercentage;
        product.StockQuantity = request.StockQuantity;
        product.MinimumStock = request.MinimumStock;
        product.MaximumStock = request.MaximumStock;
        product.StockStatus = request.StockStatus;
        product.Weight = request.Weight;
        product.Length = request.Length;
        product.Width = request.Width;
        product.Height = request.Height;
        product.MetaTitle = request.MetaTitle?.Trim();
        product.MetaDescription = request.MetaDescription?.Trim();
        product.MetaKeywords = request.MetaKeywords?.Trim();
        product.IsFeatured = request.IsFeatured;
        product.IsNewArrival = request.IsNewArrival;
        product.IsBestSeller = request.IsBestSeller;
        product.IsActive = request.IsActive;
        product.IsPublished = request.IsPublished;
        product.UpdatedAtUtc = DateTime.UtcNow;

        // Clean variants and rebuild
        db.ProductVariants.RemoveRange(product.Variants);
        product.Variants.Clear();

        if (request.Variants != null)
        {
            foreach (var v in request.Variants)
            {
                var variant = new ProductVariant
                {
                    Name = v.Name.Trim(),
                    Sku = v.Sku.Trim(),
                    Barcode = v.Barcode?.Trim(),
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    ImageUrl = v.ImageUrl,
                    IsActive = v.IsActive
                };

                foreach (var av in v.AttributeValues)
                {
                    variant.AttributeValues.Add(new ProductAttributeValue
                    {
                        ProductAttributeId = av.ProductAttributeId,
                        Value = av.Value.Trim()
                    });
                }
                product.Variants.Add(variant);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        product = await ProductQuery().FirstAsync(x => x.Id == id, cancellationToken);
        await searchIndexer.IndexProductAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        
        product.IsDeleted = true;
        product.DeletedAtUtc = DateTime.UtcNow;

        var inventories = await db.Inventories.Where(x => x.ProductId == id).ToListAsync(cancellationToken);
        foreach (var inv in inventories)
        {
            inv.IsDeleted = true;
            inv.DeletedAtUtc = DateTime.UtcNow;
        }
        
        await db.SaveChangesAsync(cancellationToken);
        await searchIndexer.DeleteProductAsync(id, cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted, cancellationToken);
        if (product is null) return false;

        product.IsDeleted = false;
        product.DeletedAtUtc = null;
        product.DeletedBy = null;

        var inventories = await db.Inventories.IgnoreQueryFilters().Where(x => x.ProductId == id && x.IsDeleted).ToListAsync(cancellationToken);
        foreach (var inv in inventories)
        {
            inv.IsDeleted = false;
            inv.DeletedAtUtc = null;
        }

        await db.SaveChangesAsync(cancellationToken);
        await searchIndexer.IndexProductAsync(product, cancellationToken);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        product.IsActive = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        product.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        product.IsPublished = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnpublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        product.IsPublished = false;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProductDto?> DuplicateAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(x => x.Images)
            .Include(x => x.Variants)
                .ThenInclude(v => v.AttributeValues)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return null;

        var uniqueSuffix = Guid.NewGuid().ToString()[..6];
        var duplicateSku = $"{product.Sku}-DUP-{uniqueSuffix}";
        var duplicateSlug = $"{product.Slug}-dup-{uniqueSuffix}";

        var duplicated = new Product
        {
            Name = $"{product.Name} (Copy)",
            Sku = duplicateSku,
            Barcode = product.Barcode != null ? $"{product.Barcode}-DUP" : null,
            Slug = duplicateSlug,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            CategoryId = product.CategoryId,
            SubCategoryId = product.SubCategoryId,
            BrandId = product.BrandId,
            CostPrice = product.CostPrice,
            Price = product.Price,
            DiscountType = product.DiscountType,
            DiscountValue = product.DiscountValue,
            DiscountStartDate = product.DiscountStartDate,
            DiscountEndDate = product.DiscountEndDate,
            TaxPercentage = product.TaxPercentage,
            StockQuantity = product.StockQuantity,
            MinimumStock = product.MinimumStock,
            MaximumStock = product.MaximumStock,
            StockStatus = product.StockStatus,
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            MetaKeywords = product.MetaKeywords,
            IsFeatured = product.IsFeatured,
            IsNewArrival = product.IsNewArrival,
            IsBestSeller = product.IsBestSeller,
            IsActive = product.IsActive,
            IsPublished = false // Always unpublish copies
        };

        foreach (var img in product.Images)
        {
            duplicated.Images.Add(new ProductImage
            {
                Url = img.Url,
                PublicId = img.PublicId,
                IsPrimary = img.IsPrimary,
                DisplayOrder = img.DisplayOrder
            });
        }

        foreach (var v in product.Variants)
        {
            var newVar = new ProductVariant
            {
                Name = v.Name,
                Sku = $"{v.Sku}-DUP-{uniqueSuffix}",
                Barcode = v.Barcode != null ? $"{v.Barcode}-DUP" : null,
                Price = v.Price,
                StockQuantity = v.StockQuantity,
                ImageUrl = v.ImageUrl,
                IsActive = v.IsActive
            };

            foreach (var av in v.AttributeValues)
            {
                newVar.AttributeValues.Add(new ProductAttributeValue
                {
                    ProductAttributeId = av.ProductAttributeId,
                    Value = av.Value
                });
            }
            duplicated.Variants.Add(newVar);
        }

        db.Products.Add(duplicated);
        await db.SaveChangesAsync(cancellationToken);

        duplicated = await ProductQuery().FirstAsync(x => x.Id == duplicated.Id, cancellationToken);
        await searchIndexer.IndexProductAsync(duplicated, cancellationToken);
        return duplicated.ToDto();
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

        var maxOrder = product.Images.Count > 0 ? product.Images.Max(x => x.DisplayOrder) : 0;

        var image = new ProductImage 
        { 
            ProductId = productId, 
            Url = upload.Url, 
            PublicId = upload.PublicId, 
            IsPrimary = isPrimary, 
            DisplayOrder = maxOrder + 1 
        };
        db.ProductImages.Add(image);
        await db.SaveChangesAsync(cancellationToken);
        return new ProductImageDto(image.Id, image.Url, image.IsPrimary, image.DisplayOrder);
    }

    public async Task<bool> DeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, cancellationToken);
        if (image is null) return false;

        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReorderImagesAsync(Guid productId, List<ImageReorderRequest> requests, CancellationToken cancellationToken)
    {
        var images = await db.ProductImages.Where(x => x.ProductId == productId).ToListAsync(cancellationToken);
        if (images.Count == 0) return false;

        foreach (var req in requests)
        {
            var img = images.FirstOrDefault(x => x.Id == req.ImageId);
            if (img != null)
            {
                img.DisplayOrder = req.DisplayOrder;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
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

    public async Task<IReadOnlyList<ProductAttributeDto>> GetAttributesAsync(CancellationToken cancellationToken) =>
        await db.ProductAttributes.AsNoTracking().OrderBy(x => x.Name).Select(x => new ProductAttributeDto(x.Id, x.Name)).ToListAsync(cancellationToken);

    public async Task<ProductAttributeDto> CreateAttributeAsync(UpsertProductAttributeRequest request, CancellationToken cancellationToken)
    {
        var attr = new ProductAttribute { Name = request.Name.Trim() };
        db.ProductAttributes.Add(attr);
        await db.SaveChangesAsync(cancellationToken);
        return new ProductAttributeDto(attr.Id, attr.Name);
    }

    public async Task<IReadOnlyList<ProductDto>> GetSuggestionsAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<ProductDto>();
        var term = $"%{query.Trim().ToLower()}%";

        var products = await ProductQuery()
            .Where(x => x.IsActive && x.IsPublished && 
                        (EF.Functions.Like(x.Name.ToLower(), term) || 
                         EF.Functions.Like(x.Sku.ToLower(), term)))
            .Take(8)
            .ToListAsync(cancellationToken);

        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetRelatedAsync(Guid productId, int count, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([productId], cancellationToken);
        if (product == null) return new List<ProductDto>();

        var products = await ProductQuery()
            .Where(x => x.CategoryId == product.CategoryId && x.Id != productId && x.IsActive && x.IsPublished)
            .Take(count)
            .ToListAsync(cancellationToken);

        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<string>> GetUserSearchHistoryAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue) return new List<string>();
        return await db.SearchHistories
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => x.QueryText)
            .Distinct()
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPopularSearchesAsync(int count, CancellationToken cancellationToken)
    {
        return await db.SearchHistories
            .GroupBy(x => x.QueryText)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveSearchQueryAsync(Guid? userId, string queryText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queryText)) return;
        var cleanQuery = queryText.Trim();

        var cutOff = DateTime.UtcNow.AddMinutes(-5);
        var exists = await db.SearchHistories
            .AnyAsync(x => x.UserId == userId && x.QueryText == cleanQuery && x.CreatedAtUtc >= cutOff, cancellationToken);

        if (!exists)
        {
            var history = new SearchHistory
            {
                UserId = userId,
                QueryText = cleanQuery
            };
            db.SearchHistories.Add(history);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private IQueryable<Product> ProductQuery() => db.Products
        .Include(x => x.Category)
        .Include(x => x.SubCategory)
        .Include(x => x.Brand)
        .Include(x => x.Images)
        .Include(x => x.Reviews)
        .Include(x => x.Variants)
            .ThenInclude(v => v.AttributeValues)
                .ThenInclude(av => av.ProductAttribute);
}

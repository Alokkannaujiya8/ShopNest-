using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class ProductManagementService(
    ShopNestDbContext db,
    IMapper mapper,
    IImageStorageService imageStorage) : IProductManagementService
{
    // ==========================================
    // QUERIES
    // ==========================================
    public async Task<PagedResult<AdminProductDto>> GetProductsAsync(
        string? query,
        Guid? categoryId,
        Guid? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        string? stockStatus,
        bool? isActive,
        bool? isPublished,
        bool? isFeatured,
        bool? isNewArrival,
        bool? isBestSeller,
        bool? isDeleted,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var dbQuery = db.Products
            .IgnoreQueryFilters()
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .AsNoTracking();

        // 1. Soft Deleted status filter
        if (isDeleted.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsDeleted == isDeleted.Value);
        }
        else
        {
            dbQuery = dbQuery.Where(x => !x.IsDeleted);
        }

        // 2. Search Text
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(x => 
                x.Name.ToLower().Contains(term) ||
                x.Sku.ToLower().Contains(term) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(term)) ||
                (x.Brand != null && x.Brand.Name.ToLower().Contains(term)) ||
                x.Category.Name.ToLower().Contains(term) ||
                x.Slug.ToLower().Contains(term)
            );
        }

        // 3. Filters
        if (categoryId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CategoryId == categoryId.Value || x.SubCategoryId == categoryId.Value);
        }
        if (brandId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.BrandId == brandId.Value);
        }
        if (minPrice.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Price <= maxPrice.Value);
        }
        if (!string.IsNullOrWhiteSpace(stockStatus))
        {
            dbQuery = dbQuery.Where(x => x.StockStatus == stockStatus);
        }
        if (isActive.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsActive == isActive.Value);
        }
        if (isPublished.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsPublished == isPublished.Value);
        }
        if (isFeatured.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsFeatured == isFeatured.Value);
        }
        if (isNewArrival.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsNewArrival == isNewArrival.Value);
        }
        if (isBestSeller.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsBestSeller == isBestSeller.Value);
        }

        // 4. Sorting
        dbQuery = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? dbQuery.OrderByDescending(x => x.Name) : dbQuery.OrderBy(x => x.Name),
            "price" => descending ? dbQuery.OrderByDescending(x => x.Price) : dbQuery.OrderBy(x => x.Price),
            "stock" => descending ? dbQuery.OrderByDescending(x => x.StockQuantity) : dbQuery.OrderBy(x => x.StockQuantity),
            "createddate" => descending ? dbQuery.OrderByDescending(x => x.CreatedAtUtc) : dbQuery.OrderBy(x => x.CreatedAtUtc),
            "updateddate" => descending ? dbQuery.OrderByDescending(x => x.UpdatedAtUtc) : dbQuery.OrderBy(x => x.UpdatedAtUtc),
            "bestselling" => dbQuery.OrderByDescending(x => x.IsBestSeller).ThenBy(x => x.Name),
            _ => dbQuery.OrderByDescending(x => x.CreatedAtUtc)
        };

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var mapped = mapper.Map<IReadOnlyList<AdminProductDto>>(items);
        return new PagedResult<AdminProductDto>(mapped, page, pageSize, totalCount);
    }

    public async Task<AdminProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .IgnoreQueryFilters()
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return mapper.Map<AdminProductDto>(product);
    }

    public async Task<AdminProductDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .IgnoreQueryFilters()
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        return mapper.Map<AdminProductDto>(product);
    }

    public async Task<IReadOnlyList<AdminProductDto>> GetFeaturedProductsAsync(int count, CancellationToken cancellationToken)
    {
        var list = await db.Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.IsActive && x.IsPublished && x.IsFeatured)
            .OrderBy(x => x.Name)
            .Take(count)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<AdminProductDto>>(list);
    }

    public async Task<IReadOnlyList<AdminProductDto>> GetBestSellerProductsAsync(int count, CancellationToken cancellationToken)
    {
        var list = await db.Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.IsActive && x.IsPublished && x.IsBestSeller)
            .OrderBy(x => x.Name)
            .Take(count)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<AdminProductDto>>(list);
    }

    public async Task<IReadOnlyList<AdminProductDto>> GetNewArrivalProductsAsync(int count, CancellationToken cancellationToken)
    {
        var list = await db.Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.IsActive && x.IsPublished && x.IsNewArrival)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<AdminProductDto>>(list);
    }

    // ==========================================
    // COMMANDS
    // ==========================================
    public async Task<AdminProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var slug = MappingExtensions.Slugify(name);

        var sku = request.Sku.Trim();
        var barcode = request.Barcode?.Trim();

        // Unique validations
        if (await db.Products.AnyAsync(x => x.Sku == sku, cancellationToken))
        {
            throw new InvalidOperationException($"SKU '{sku}' is already assigned to another product.");
        }
        if (!string.IsNullOrWhiteSpace(barcode) && await db.Products.AnyAsync(x => x.Barcode == barcode, cancellationToken))
        {
            throw new InvalidOperationException($"Barcode '{barcode}' is already assigned to another product.");
        }
        if (await db.Products.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";
        }

        var product = new Product
        {
            Name = name,
            Slug = slug,
            Description = request.Description.Trim(),
            ShortDescription = request.ShortDescription?.Trim(),
            Sku = sku,
            Barcode = barcode,
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
            StockStatus = CalculateStockStatus(request.StockQuantity, request.MinimumStock),
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            MetaTitle = request.MetaTitle?.Trim() ?? name,
            MetaDescription = request.MetaDescription?.Trim(),
            MetaKeywords = request.MetaKeywords?.Trim(),
            IsActive = true,
            IsPublished = false,
            IsFeatured = request.IsFeatured,
            IsNewArrival = request.IsNewArrival,
            IsBestSeller = request.IsBestSeller,
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            BrandId = request.BrandId,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Add Variants
        if (request.Variants != null)
        {
            foreach (var vReq in request.Variants)
            {
                if (await db.ProductVariants.AnyAsync(x => x.Sku == vReq.Sku.Trim(), cancellationToken))
                {
                    throw new InvalidOperationException($"Variant SKU '{vReq.Sku}' already exists.");
                }
                product.Variants.Add(new ProductVariant
                {
                    Name = vReq.Name.Trim(),
                    Sku = vReq.Sku.Trim(),
                    Barcode = vReq.Barcode?.Trim(),
                    Price = vReq.Price,
                    StockQuantity = vReq.StockQuantity,
                    IsActive = vReq.IsActive,
                    ImageUrl = vReq.ImageUrl,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        // Add ProductCategory join
        product.ProductCategories.Add(new ProductCategory { CategoryId = request.CategoryId });
        if (request.SubCategoryId.HasValue)
        {
            product.ProductCategories.Add(new ProductCategory { CategoryId = request.SubCategoryId.Value });
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        // Fetch back with navigation properties populated
        return (await GetProductByIdAsync(product.Id, cancellationToken))!;
    }

    public async Task<AdminProductDto?> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(x => x.Variants)
            .Include(x => x.ProductCategories)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null) return null;

        var name = request.Name.Trim();
        var slug = MappingExtensions.Slugify(name);
        var sku = request.Sku.Trim();
        var barcode = request.Barcode?.Trim();

        // Unique constraints checks
        if (await db.Products.AnyAsync(x => x.Sku == sku && x.Id != id, cancellationToken))
        {
            throw new InvalidOperationException($"SKU '{sku}' is already assigned to another product.");
        }
        if (!string.IsNullOrWhiteSpace(barcode) && await db.Products.AnyAsync(x => x.Barcode == barcode && x.Id != id, cancellationToken))
        {
            throw new InvalidOperationException($"Barcode '{barcode}' is already assigned to another product.");
        }
        if (await db.Products.AnyAsync(x => x.Slug == slug && x.Id != id, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";
        }

        product.Name = name;
        product.Slug = slug;
        product.Description = request.Description.Trim();
        product.ShortDescription = request.ShortDescription?.Trim();
        product.Sku = sku;
        product.Barcode = barcode;
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
        product.StockStatus = CalculateStockStatus(request.StockQuantity, request.MinimumStock);
        product.Weight = request.Weight;
        product.Length = request.Length;
        product.Width = request.Width;
        product.Height = request.Height;
        product.MetaTitle = request.MetaTitle?.Trim() ?? name;
        product.MetaDescription = request.MetaDescription?.Trim();
        product.MetaKeywords = request.MetaKeywords?.Trim();
        product.IsActive = request.IsActive;
        product.IsPublished = request.IsPublished;
        product.IsFeatured = request.IsFeatured;
        product.IsNewArrival = request.IsNewArrival;
        product.IsBestSeller = request.IsBestSeller;
        product.CategoryId = request.CategoryId;
        product.SubCategoryId = request.SubCategoryId;
        product.BrandId = request.BrandId;
        product.UpdatedAtUtc = DateTime.UtcNow;

        // Categories mapping sync
        db.ProductCategories.RemoveRange(product.ProductCategories);
        product.ProductCategories.Clear();
        product.ProductCategories.Add(new ProductCategory { ProductId = id, CategoryId = request.CategoryId });
        if (request.SubCategoryId.HasValue)
        {
            product.ProductCategories.Add(new ProductCategory { ProductId = id, CategoryId = request.SubCategoryId.Value });
        }

        // Variants sync
        var incomingIds = request.Variants.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var variantsToRemove = product.Variants.Where(x => !incomingIds.Contains(x.Id)).ToList();
        foreach (var toRemove in variantsToRemove)
        {
            db.ProductVariants.Remove(toRemove);
        }

        foreach (var vReq in request.Variants)
        {
            if (vReq.Id.HasValue)
            {
                var existing = product.Variants.FirstOrDefault(x => x.Id == vReq.Id.Value);
                if (existing != null)
                {
                    existing.Name = vReq.Name.Trim();
                    existing.Sku = vReq.Sku.Trim();
                    existing.Barcode = vReq.Barcode?.Trim();
                    existing.Price = vReq.Price;
                    existing.StockQuantity = vReq.StockQuantity;
                    existing.IsActive = vReq.IsActive;
                    existing.ImageUrl = vReq.ImageUrl;
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                if (await db.ProductVariants.AnyAsync(x => x.Sku == vReq.Sku.Trim(), cancellationToken))
                {
                    throw new InvalidOperationException($"Variant SKU '{vReq.Sku}' already exists.");
                }
                product.Variants.Add(new ProductVariant
                {
                    Name = vReq.Name.Trim(),
                    Sku = vReq.Sku.Trim(),
                    Barcode = vReq.Barcode?.Trim(),
                    Price = vReq.Price,
                    StockQuantity = vReq.StockQuantity,
                    IsActive = vReq.IsActive,
                    ImageUrl = vReq.ImageUrl,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetProductByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return false;

        db.Products.Remove(product); // Soft delete
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null || !product.IsDeleted) return false;

        product.IsDeleted = false;
        product.DeletedAtUtc = null;
        product.DeletedBy = null;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PublishProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return false;

        product.IsPublished = true;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnpublishProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return false;

        product.IsPublished = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleProductActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return false;

        product.IsActive = isActive;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AdminProductDto?> DuplicateProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var original = await db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (original is null) return null;

        var guidSuffix = Guid.NewGuid().ToString()[..6];
        var copySku = $"{original.Sku}-COPY-{guidSuffix.ToUpper()}";
        var copySlug = $"{original.Slug}-copy-{guidSuffix}";

        var clone = new Product
        {
            Name = $"{original.Name} (Copy)",
            Slug = copySlug,
            Description = original.Description,
            ShortDescription = original.ShortDescription,
            Sku = copySku,
            Barcode = null,
            CostPrice = original.CostPrice,
            Price = original.Price,
            DiscountType = original.DiscountType,
            DiscountValue = original.DiscountValue,
            DiscountStartDate = original.DiscountStartDate,
            DiscountEndDate = original.DiscountEndDate,
            TaxPercentage = original.TaxPercentage,
            StockQuantity = original.StockQuantity,
            MinimumStock = original.MinimumStock,
            MaximumStock = original.MaximumStock,
            StockStatus = original.StockStatus,
            Weight = original.Weight,
            Length = original.Length,
            Width = original.Width,
            Height = original.Height,
            MetaTitle = $"{original.MetaTitle} (Copy)",
            MetaDescription = original.MetaDescription,
            MetaKeywords = original.MetaKeywords,
            IsActive = true,
            IsPublished = false,
            IsFeatured = original.IsFeatured,
            IsNewArrival = true,
            IsBestSeller = false,
            CategoryId = original.CategoryId,
            SubCategoryId = original.SubCategoryId,
            BrandId = original.BrandId,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Clone Images
        foreach (var img in original.Images)
        {
            clone.Images.Add(new ProductImage
            {
                Url = img.Url,
                PublicId = img.PublicId,
                IsPrimary = img.IsPrimary,
                DisplayOrder = img.DisplayOrder,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        // Clone Variants
        foreach (var v in original.Variants)
        {
            var variantSuffix = Guid.NewGuid().ToString()[..4];
            clone.Variants.Add(new ProductVariant
            {
                Name = v.Name,
                Sku = $"{v.Sku}-COPY-{variantSuffix.ToUpper()}",
                Barcode = null,
                Price = v.Price,
                StockQuantity = v.StockQuantity,
                IsActive = v.IsActive,
                ImageUrl = v.ImageUrl,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        clone.ProductCategories.Add(new ProductCategory { CategoryId = original.CategoryId });
        if (original.SubCategoryId.HasValue)
        {
            clone.ProductCategories.Add(new ProductCategory { CategoryId = original.SubCategoryId.Value });
        }

        db.Products.Add(clone);
        await db.SaveChangesAsync(cancellationToken);

        return await GetProductByIdAsync(clone.Id, cancellationToken);
    }

    // ==========================================
    // IMAGES
    // ==========================================
    public async Task<IReadOnlyList<ProductImageDto>> UploadImagesAsync(Guid id, List<IFormFile> files, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null) return [];

        var startingOrder = product.Images.Count > 0 ? product.Images.Max(x => x.DisplayOrder) + 1 : 0;
        var uploadedList = new List<ProductImageDto>();

        foreach (var file in files)
        {
            using var stream = file.OpenReadStream();
            var uploadResult = await imageStorage.UploadAsync(stream, file.FileName, cancellationToken);

            var isPrimary = product.Images.Count == 0 && uploadedList.Count == 0;
            var img = new ProductImage
            {
                ProductId = id,
                Url = uploadResult.Url,
                PublicId = uploadResult.PublicId,
                IsPrimary = isPrimary,
                DisplayOrder = startingOrder++
            };

            db.ProductImages.Add(img);
            product.Images.Add(img);

            uploadedList.Add(new ProductImageDto(img.Id, img.Url, img.IsPrimary, img.DisplayOrder));
        }

        await db.SaveChangesAsync(cancellationToken);
        return uploadedList;
    }

    public async Task<bool> DeleteImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var img = await db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == id, cancellationToken);
        if (img is null) return false;

        // Clean up from storage
        if (!string.IsNullOrWhiteSpace(img.PublicId))
        {
            await imageStorage.DeleteAsync(img.PublicId, cancellationToken);
        }

        db.ProductImages.Remove(img);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReorderImagesAsync(Guid id, List<Guid> imageIds, CancellationToken cancellationToken)
    {
        var images = await db.ProductImages.Where(x => x.ProductId == id).ToListAsync(cancellationToken);
        if (!images.Any()) return false;

        for (int i = 0; i < imageIds.Count; i++)
        {
            var target = images.FirstOrDefault(x => x.Id == imageIds[i]);
            if (target != null)
            {
                target.DisplayOrder = i;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ==========================================
    // HELPERS
    // ==========================================
    private static string CalculateStockStatus(int stockQuantity, int minStock)
    {
        if (stockQuantity <= 0) return "OutOfStock";
        if (stockQuantity <= minStock) return "LowStock";
        return "InStock";
    }
}

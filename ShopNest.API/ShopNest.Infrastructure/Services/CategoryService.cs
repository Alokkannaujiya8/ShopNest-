using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class CategoryService(
    ShopNestDbContext db,
    IMapper mapper,
    IImageStorageService imageStorage) : ICategoryService
{
    // ==========================================
    // QUERIES
    // ==========================================
    public async Task<PagedResult<AdminCategoryDto>> GetCategoriesAsync(
        string? search,
        bool? isActive,
        bool? isFeatured,
        Guid? parentId,
        bool? isDeleted,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = db.Categories
            .IgnoreQueryFilters() // Support filtering by soft-deleted state
            .Include(x => x.Parent)
            .Include(x => x.Children)
            .AsNoTracking();

        // 1. Filter by deleted status
        if (isDeleted.HasValue)
        {
            query = query.Where(x => x.IsDeleted == isDeleted.Value);
        }
        else
        {
            query = query.Where(x => !x.IsDeleted);
        }

        // 2. Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => 
                x.Name.ToLower().Contains(term) ||
                x.Slug.ToLower().Contains(term)
            );
        }

        // 3. Status Filters
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }
        if (isFeatured.HasValue)
        {
            query = query.Where(x => x.IsFeatured == isFeatured.Value);
        }
        if (parentId.HasValue)
        {
            query = query.Where(x => x.ParentId == parentId.Value);
        }

        // 4. Sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "displayorder" => descending ? query.OrderByDescending(x => x.DisplayOrder) : query.OrderBy(x => x.DisplayOrder),
            "createddate" => descending ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            "updateddate" => descending ? query.OrderByDescending(x => x.UpdatedAtUtc) : query.OrderBy(x => x.UpdatedAtUtc),
            _ => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var mapped = mapper.Map<IReadOnlyList<AdminCategoryDto>>(items);
        return new PagedResult<AdminCategoryDto>(mapped, page, pageSize, totalCount);
    }

    public async Task<AdminCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .IgnoreQueryFilters()
            .Include(x => x.Parent)
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return mapper.Map<AdminCategoryDto>(category);
    }

    public async Task<IReadOnlyList<CategoryNodeDto>> GetCategoryTreeAsync(CancellationToken cancellationToken)
    {
        var allCategories = await db.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        // Find root nodes
        var roots = allCategories.Where(x => x.ParentId == null).ToList();
        var tree = new List<CategoryNodeDto>();

        foreach (var root in roots)
        {
            tree.Add(BuildNode(root, allCategories));
        }

        return tree;
    }

    private CategoryNodeDto BuildNode(Category category, List<Category> allCategories)
    {
        var node = new CategoryNodeDto(
            category.Id,
            category.Name,
            category.Slug,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsActive,
            new List<CategoryNodeDto>()
        );

        var children = allCategories
            .Where(x => x.ParentId == category.Id)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name);

        foreach (var child in children)
        {
            node.Children.Add(BuildNode(child, allCategories));
        }

        return node;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetParentCategoriesAsync(CancellationToken cancellationToken)
    {
        var roots = await db.Categories
            .Where(x => x.ParentId == null && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CategoryDto(x.Id, x.Name, x.Slug))
            .ToListAsync(cancellationToken);

        return roots;
    }

    // ==========================================
    // COMMANDS
    // ==========================================
    public async Task<AdminCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var slug = MappingExtensions.Slugify(name);

        var exists = await db.Categories.AnyAsync(x => x.Slug == slug, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("A category with a similar name or slug already exists.");
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await db.Categories.AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);
            if (!parentExists) throw new InvalidOperationException("Parent category not found.");
        }

        var category = new Category
        {
            Name = name,
            Slug = slug,
            Description = request.Description?.Trim(),
            ShortDescription = request.ShortDescription?.Trim(),
            ParentId = request.ParentId,
            DisplayOrder = request.DisplayOrder,
            IsFeatured = request.IsFeatured,
            IsActive = true,
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            MetaKeywords = request.MetaKeywords?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<AdminCategoryDto>(category);
    }

    public async Task<AdminCategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (category is null) return null;

        var name = request.Name.Trim();
        var slug = MappingExtensions.Slugify(name);

        var exists = await db.Categories.AnyAsync(x => x.Slug == slug && x.Id != id, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Another category with a similar name or slug already exists.");
        }

        // Validate circular references
        if (request.ParentId.HasValue)
        {
            await ValidateParentCycleAsync(id, request.ParentId.Value, cancellationToken);
        }

        category.Name = name;
        category.Slug = slug;
        category.Description = request.Description?.Trim();
        category.ShortDescription = request.ShortDescription?.Trim();
        category.ParentId = request.ParentId;
        category.DisplayOrder = request.DisplayOrder;
        category.IsFeatured = request.IsFeatured;
        category.IsActive = request.IsActive;
        category.MetaTitle = request.MetaTitle?.Trim();
        category.MetaDescription = request.MetaDescription?.Trim();
        category.MetaKeywords = request.MetaKeywords?.Trim();
        category.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<AdminCategoryDto>(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null) return false;

        // Prevent deletion if the category has active child subcategories
        var activeChildren = category.Children.Any(x => !x.IsDeleted);
        if (activeChildren)
        {
            throw new InvalidOperationException("Cannot delete a parent category containing active subcategories. Delete or re-assign children first.");
        }

        db.Categories.Remove(category); // Soft-delete
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleCategoryActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return false;

        category.IsActive = isActive;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null || !category.IsDeleted) return false;

        category.IsDeleted = false;
        category.DeletedAtUtc = null;
        category.DeletedBy = null;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ==========================================
    // UPLOADS
    // ==========================================
    public async Task<string?> UploadImageAsync(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return null;

        // Clean up previous image if exists
        if (!string.IsNullOrWhiteSpace(category.ImagePublicId))
        {
            await imageStorage.DeleteAsync(category.ImagePublicId, cancellationToken);
        }

        using var stream = file.OpenReadStream();
        var result = await imageStorage.UploadAsync(stream, file.FileName, cancellationToken);

        category.ImageUrl = result.Url;
        category.ImagePublicId = result.PublicId;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return result.Url;
    }

    public async Task<string?> UploadBannerAsync(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return null;

        // Clean up previous banner if exists
        if (!string.IsNullOrWhiteSpace(category.BannerPublicId))
        {
            await imageStorage.DeleteAsync(category.BannerPublicId, cancellationToken);
        }

        using var stream = file.OpenReadStream();
        var result = await imageStorage.UploadAsync(stream, file.FileName, cancellationToken);

        category.BannerUrl = result.Url;
        category.BannerPublicId = result.PublicId;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return result.Url;
    }

    // ==========================================
    // VALIDATIONS
    // ==========================================
    private async Task ValidateParentCycleAsync(Guid categoryId, Guid parentId, CancellationToken cancellationToken)
    {
        if (categoryId == parentId)
        {
            throw new InvalidOperationException("A category cannot be its own parent.");
        }

        var currentParentId = parentId;
        while (currentParentId != Guid.Empty)
        {
            var parent = await db.Categories
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.Id == currentParentId)
                .Select(x => new { x.ParentId })
                .FirstOrDefaultAsync(cancellationToken);

            if (parent == null) break;

            if (parent.ParentId == categoryId)
            {
                throw new InvalidOperationException("Proposed parent category is a subcategory of this category, which causes a circular reference cycle.");
            }

            currentParentId = parent.ParentId ?? Guid.Empty;
        }
    }
}

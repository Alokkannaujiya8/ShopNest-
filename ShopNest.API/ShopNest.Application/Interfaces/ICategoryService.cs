using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface ICategoryService
{
    // Queries
    Task<PagedResult<AdminCategoryDto>> GetCategoriesAsync(
        string? search,
        bool? isActive,
        bool? isFeatured,
        Guid? parentId,
        bool? isDeleted,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryNodeDto>> GetCategoryTreeAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryDto>> GetParentCategoriesAsync(CancellationToken cancellationToken);

    // Commands
    Task<AdminCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<AdminCategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ToggleCategoryActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<bool> RestoreCategoryAsync(Guid id, CancellationToken cancellationToken);

    // File Uploads
    Task<string?> UploadImageAsync(Guid id, IFormFile file, CancellationToken cancellationToken);
    Task<string?> UploadBannerAsync(Guid id, IFormFile file, CancellationToken cancellationToken);
}

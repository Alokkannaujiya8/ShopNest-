using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IProductManagementService
{
    // Queries
    Task<PagedResult<AdminProductDto>> GetProductsAsync(
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
        CancellationToken cancellationToken);

    Task<AdminProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminProductDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminProductDto>> GetFeaturedProductsAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminProductDto>> GetBestSellerProductsAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminProductDto>> GetNewArrivalProductsAsync(int count, CancellationToken cancellationToken);

    // Commands
    Task<AdminProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<AdminProductDto?> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> RestoreProductAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> PublishProductAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> UnpublishProductAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ToggleProductActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<AdminProductDto?> DuplicateProductAsync(Guid id, CancellationToken cancellationToken);

    // Image Commands
    Task<IReadOnlyList<ProductImageDto>> UploadImagesAsync(Guid id, List<IFormFile> files, CancellationToken cancellationToken);
    Task<bool> DeleteImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken);
    Task<bool> ReorderImagesAsync(Guid id, List<Guid> imageIds, CancellationToken cancellationToken);
}

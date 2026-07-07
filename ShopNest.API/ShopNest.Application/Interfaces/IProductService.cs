using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IProductService
{
    // Queries
    Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken);
    Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductDto>> GetBestSellersAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductDto>> GetNewArrivalsAsync(int count, CancellationToken cancellationToken);

    // CRUD
    Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken);
    Task<ProductDto?> UpdateAsync(Guid id, UpsertProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> PublishAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> UnpublishAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto?> DuplicateAsync(Guid id, CancellationToken cancellationToken);

    // Images
    Task<ProductImageDto?> UploadImageAsync(Guid productId, IFormFile file, bool isPrimary, CancellationToken cancellationToken);
    Task<bool> DeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken);
    Task<bool> ReorderImagesAsync(Guid productId, List<ImageReorderRequest> requests, CancellationToken cancellationToken);

    // Categories & Attributes
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<CategoryDto> CreateCategoryAsync(UpsertCategoryRequest request, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<ProductAttributeDto>> GetAttributesAsync(CancellationToken cancellationToken);
    Task<ProductAttributeDto> CreateAttributeAsync(UpsertProductAttributeRequest request, CancellationToken cancellationToken);

    // Advanced Catalog Queries
    Task<IReadOnlyList<ProductDto>> GetSuggestionsAsync(string query, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductDto>> GetRelatedAsync(Guid productId, int count, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetUserSearchHistoryAsync(Guid? userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetPopularSearchesAsync(int count, CancellationToken cancellationToken);
    Task SaveSearchQueryAsync(Guid? userId, string queryText, CancellationToken cancellationToken);
}

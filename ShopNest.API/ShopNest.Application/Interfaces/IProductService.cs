using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken);
    Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken);
    Task<ProductDto?> UpdateAsync(Guid id, UpsertProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductImageDto?> UploadImageAsync(Guid productId, IFormFile file, bool isPrimary, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<CategoryDto> CreateCategoryAsync(UpsertCategoryRequest request, CancellationToken cancellationToken);
}

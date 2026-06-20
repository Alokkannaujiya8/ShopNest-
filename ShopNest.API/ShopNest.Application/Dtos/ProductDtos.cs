namespace ShopNest.Application.Dtos;

public sealed record CategoryDto(Guid Id, string Name, string Slug);
public sealed record ProductImageDto(Guid Id, string Url, bool IsPrimary);
public sealed record ProductDto(Guid Id, string Name, string Slug, string Description, decimal Price, int StockQuantity, bool IsActive, CategoryDto Category, IReadOnlyList<ProductImageDto> Images);
public sealed record UpsertCategoryRequest(string Name);
public sealed record UpsertProductRequest(string Name, string Description, decimal Price, int StockQuantity, Guid CategoryId, bool IsActive = true);
public sealed record ProductSearchRequest(string? Query, Guid? CategoryId, decimal? MinPrice, decimal? MaxPrice, bool? InStock, int Page = 1, int PageSize = 20);
public sealed record ImageUploadResult(string Url, string PublicId);

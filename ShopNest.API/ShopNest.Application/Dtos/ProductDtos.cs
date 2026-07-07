namespace ShopNest.Application.Dtos;

// Original basic DTOs (Preserved to prevent breaking changes)
public sealed record CategoryDto(Guid Id, string Name, string Slug);
public sealed record ProductImageDto(Guid Id, string Url, bool IsPrimary, int DisplayOrder = 0);

public sealed record ProductAttributeValueDto(
    Guid Id,
    Guid ProductAttributeId,
    string AttributeName,
    string Value
);

public sealed record ProductVariantDto(
    Guid Id,
    string Name,
    string Sku,
    string? Barcode,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive,
    IReadOnlyList<ProductAttributeValueDto> AttributeValues
);

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string? Barcode,
    string Slug,
    string? ShortDescription,
    string Description,
    Guid CategoryId,
    CategoryDto Category,
    Guid? SubCategoryId,
    CategoryDto? SubCategory,
    Guid? BrandId,
    string? BrandName,
    decimal CostPrice,
    decimal Price,
    string? DiscountType,
    decimal DiscountValue,
    DateTime? DiscountStartDate,
    DateTime? DiscountEndDate,
    decimal TaxPercentage,
    int StockQuantity,
    int MinimumStock,
    int MaximumStock,
    string StockStatus,
    decimal Weight,
    decimal Length,
    decimal Width,
    decimal Height,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsBestSeller,
    bool IsActive,
    bool IsPublished,
    double AverageRating,
    int ReviewCount,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<ProductVariantDto> Variants
);

public sealed record UpsertCategoryRequest(string Name);
public sealed record UpsertProductRequest(
    string Name,
    string Description,
    string? ShortDescription,
    string Sku,
    string? Barcode,
    decimal CostPrice,
    decimal Price,
    string? DiscountType,
    decimal DiscountValue,
    DateTime? DiscountStartDate,
    DateTime? DiscountEndDate,
    decimal TaxPercentage,
    int StockQuantity,
    int MinimumStock,
    int MaximumStock,
    string StockStatus,
    decimal Weight,
    decimal Length,
    decimal Width,
    decimal Height,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsBestSeller,
    bool IsActive,
    bool IsPublished,
    Guid CategoryId,
    Guid? SubCategoryId,
    Guid? BrandId,
    List<UpsertVariantRequest> Variants
);
public sealed record ProductSearchRequest(
    string? Query,
    Guid? CategoryId,
    Guid? SubCategoryId,
    Guid? BrandId,
    decimal? MinPrice,
    decimal? MaxPrice,
    double? MinRating,
    string? Color,
    string? Size,
    string? Material,
    string? StockStatus,
    string? SortBy,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null,
    bool? IsPublished = null,
    bool? IsFeatured = null,
    bool? IsNewArrival = null,
    bool? IsBestSeller = null,
    bool? IsDeleted = null
);
public sealed record ImageUploadResult(string Url, string PublicId);

// Administrative Rich DTOs
public sealed record AdminProductVariantDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string Sku,
    string? Barcode,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    string? ImageUrl
);

public sealed record AdminProductDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string? ShortDescription,
    string Sku,
    string? Barcode,
    decimal CostPrice,
    decimal Price,
    string? DiscountType,
    decimal DiscountValue,
    DateTime? DiscountStartDate,
    DateTime? DiscountEndDate,
    decimal TaxPercentage,
    int StockQuantity,
    int MinimumStock,
    int MaximumStock,
    string StockStatus,
    decimal Weight,
    decimal Length,
    decimal Width,
    decimal Height,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsActive,
    bool IsPublished,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsBestSeller,
    bool IsDeleted,
    Guid CategoryId,
    string CategoryName,
    Guid? SubCategoryId,
    string? SubCategoryName,
    Guid? BrandId,
    string? BrandName,
    DateTime CreatedAtUtc,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<AdminProductVariantDto> Variants
);

public sealed record UpsertVariantAttributeRequest(
    Guid ProductAttributeId,
    string Value
);

public sealed record UpsertVariantRequest(
    Guid? Id,
    string Name,
    string Sku,
    string? Barcode,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    string? ImageUrl,
    List<UpsertVariantAttributeRequest> AttributeValues
);

public sealed record CreateProductRequest(
    string Name,
    string Description,
    string? ShortDescription,
    string Sku,
    string? Barcode,
    decimal CostPrice,
    decimal Price,
    string? DiscountType,
    decimal DiscountValue,
    DateTime? DiscountStartDate,
    DateTime? DiscountEndDate,
    decimal TaxPercentage,
    int StockQuantity,
    int MinimumStock,
    int MaximumStock,
    decimal Weight,
    decimal Length,
    decimal Width,
    decimal Height,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsBestSeller,
    Guid CategoryId,
    Guid? SubCategoryId,
    Guid? BrandId,
    List<UpsertVariantRequest> Variants
);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    string? ShortDescription,
    string Sku,
    string? Barcode,
    decimal CostPrice,
    decimal Price,
    string? DiscountType,
    decimal DiscountValue,
    DateTime? DiscountStartDate,
    DateTime? DiscountEndDate,
    decimal TaxPercentage,
    int StockQuantity,
    int MinimumStock,
    int MaximumStock,
    decimal Weight,
    decimal Length,
    decimal Width,
    decimal Height,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsActive,
    bool IsPublished,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsBestSeller,
    Guid CategoryId,
    Guid? SubCategoryId,
    Guid? BrandId,
    List<UpsertVariantRequest> Variants
);

public sealed record ProductAttributeDto(Guid Id, string Name);
public sealed record UpsertProductAttributeRequest(string Name);
public sealed record ImageReorderRequest(Guid ImageId, int DisplayOrder);

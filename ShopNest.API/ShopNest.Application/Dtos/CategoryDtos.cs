namespace ShopNest.Application.Dtos;

public sealed record AdminCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    string? ImageUrl,
    string? BannerUrl,
    Guid? ParentId,
    string? ParentName,
    int DisplayOrder,
    bool IsFeatured,
    bool IsActive,
    bool IsDeleted,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    DateTime CreatedAtUtc,
    int ChildrenCount
);

public sealed record CategoryNodeDto(
    Guid Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    List<CategoryNodeDto> Children
);

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    string? ShortDescription,
    Guid? ParentId,
    int DisplayOrder,
    bool IsFeatured,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords
);

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? ShortDescription,
    Guid? ParentId,
    int DisplayOrder,
    bool IsFeatured,
    bool IsActive,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords
);

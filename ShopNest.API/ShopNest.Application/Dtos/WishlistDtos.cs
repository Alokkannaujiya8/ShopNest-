using System;

namespace ShopNest.Application.Dtos;

public sealed record WishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string ProductSlug,
    string? BrandName,
    string CategoryName,
    decimal Price,
    decimal OriginalPrice,
    decimal DiscountValue,
    int StockQuantity,
    string StockStatus,
    double AverageRating,
    int ReviewsCount,
    string? ImageUrl,
    DateTime CreatedAtUtc
);

public sealed record WishlistSearchRequest(
    string? Query,
    Guid? CategoryId,
    Guid? BrandId,
    string? StockStatus, // InStock, OutOfStock
    bool? IsDiscounted,
    string? SortBy, // DateAdded, Name, Price
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 10
);

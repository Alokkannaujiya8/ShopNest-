using MediatR;
using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Products;

// Queries
public sealed record GetAdminProductsQuery(
    string? Query,
    Guid? CategoryId,
    Guid? BrandId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? StockStatus,
    bool? IsActive,
    bool? IsPublished,
    bool? IsFeatured,
    bool? IsNewArrival,
    bool? IsBestSeller,
    bool? IsDeleted,
    string? SortBy,
    bool Descending,
    int Page,
    int PageSize
) : IRequest<PagedResult<AdminProductDto>>;

public sealed record GetAdminProductByIdQuery(Guid Id) : IRequest<AdminProductDto?>;
public sealed record GetAdminProductBySlugQuery(string Slug) : IRequest<AdminProductDto?>;
public sealed record GetAdminFeaturedProductsQuery(int Count) : IRequest<IReadOnlyList<AdminProductDto>>;
public sealed record GetAdminBestSellerProductsQuery(int Count) : IRequest<IReadOnlyList<AdminProductDto>>;
public sealed record GetAdminNewArrivalProductsQuery(int Count) : IRequest<IReadOnlyList<AdminProductDto>>;

// Commands
public sealed record CreateAdminProductCommand(CreateProductRequest Request) : IRequest<AdminProductDto>;
public sealed record UpdateAdminProductCommand(Guid Id, UpdateProductRequest Request) : IRequest<AdminProductDto?>;
public sealed record DeleteAdminProductCommand(Guid Id) : IRequest<bool>;
public sealed record RestoreAdminProductCommand(Guid Id) : IRequest<bool>;
public sealed record PublishAdminProductCommand(Guid Id) : IRequest<bool>;
public sealed record UnpublishAdminProductCommand(Guid Id) : IRequest<bool>;
public sealed record ToggleAdminProductActivationCommand(Guid Id, bool IsActive) : IRequest<bool>;
public sealed record DuplicateAdminProductCommand(Guid Id) : IRequest<AdminProductDto?>;

// Image Commands
public sealed record UploadAdminProductImagesCommand(Guid Id, List<IFormFile> Files) : IRequest<IReadOnlyList<ProductImageDto>>;
public sealed record DeleteAdminProductImageCommand(Guid Id, Guid ImageId) : IRequest<bool>;
public sealed record ReorderAdminProductImagesCommand(Guid Id, List<Guid> ImageIds) : IRequest<bool>;

// Handlers
public sealed class ProductAdminHandlers(IProductManagementService service) :
    IRequestHandler<GetAdminProductsQuery, PagedResult<AdminProductDto>>,
    IRequestHandler<GetAdminProductByIdQuery, AdminProductDto?>,
    IRequestHandler<GetAdminProductBySlugQuery, AdminProductDto?>,
    IRequestHandler<GetAdminFeaturedProductsQuery, IReadOnlyList<AdminProductDto>>,
    IRequestHandler<GetAdminBestSellerProductsQuery, IReadOnlyList<AdminProductDto>>,
    IRequestHandler<GetAdminNewArrivalProductsQuery, IReadOnlyList<AdminProductDto>>,
    IRequestHandler<CreateAdminProductCommand, AdminProductDto>,
    IRequestHandler<UpdateAdminProductCommand, AdminProductDto?>,
    IRequestHandler<DeleteAdminProductCommand, bool>,
    IRequestHandler<RestoreAdminProductCommand, bool>,
    IRequestHandler<PublishAdminProductCommand, bool>,
    IRequestHandler<UnpublishAdminProductCommand, bool>,
    IRequestHandler<ToggleAdminProductActivationCommand, bool>,
    IRequestHandler<DuplicateAdminProductCommand, AdminProductDto?>,
    IRequestHandler<UploadAdminProductImagesCommand, IReadOnlyList<ProductImageDto>>,
    IRequestHandler<DeleteAdminProductImageCommand, bool>,
    IRequestHandler<ReorderAdminProductImagesCommand, bool>
{
    public Task<PagedResult<AdminProductDto>> Handle(GetAdminProductsQuery query, CancellationToken cancellationToken) =>
        service.GetProductsAsync(
            query.Query, query.CategoryId, query.BrandId, query.MinPrice, query.MaxPrice, query.StockStatus,
            query.IsActive, query.IsPublished, query.IsFeatured, query.IsNewArrival, query.IsBestSeller,
            query.IsDeleted, query.SortBy, query.Descending, query.Page, query.PageSize, cancellationToken);

    public Task<AdminProductDto?> Handle(GetAdminProductByIdQuery query, CancellationToken cancellationToken) =>
        service.GetProductByIdAsync(query.Id, cancellationToken);

    public Task<AdminProductDto?> Handle(GetAdminProductBySlugQuery query, CancellationToken cancellationToken) =>
        service.GetProductBySlugAsync(query.Slug, cancellationToken);

    public Task<IReadOnlyList<AdminProductDto>> Handle(GetAdminFeaturedProductsQuery query, CancellationToken cancellationToken) =>
        service.GetFeaturedProductsAsync(query.Count, cancellationToken);

    public Task<IReadOnlyList<AdminProductDto>> Handle(GetAdminBestSellerProductsQuery query, CancellationToken cancellationToken) =>
        service.GetBestSellerProductsAsync(query.Count, cancellationToken);

    public Task<IReadOnlyList<AdminProductDto>> Handle(GetAdminNewArrivalProductsQuery query, CancellationToken cancellationToken) =>
        service.GetNewArrivalProductsAsync(query.Count, cancellationToken);

    public Task<AdminProductDto> Handle(CreateAdminProductCommand command, CancellationToken cancellationToken) =>
        service.CreateProductAsync(command.Request, cancellationToken);

    public Task<AdminProductDto?> Handle(UpdateAdminProductCommand command, CancellationToken cancellationToken) =>
        service.UpdateProductAsync(command.Id, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteAdminProductCommand command, CancellationToken cancellationToken) =>
        service.DeleteProductAsync(command.Id, cancellationToken);

    public Task<bool> Handle(RestoreAdminProductCommand command, CancellationToken cancellationToken) =>
        service.RestoreProductAsync(command.Id, cancellationToken);

    public Task<bool> Handle(PublishAdminProductCommand command, CancellationToken cancellationToken) =>
        service.PublishProductAsync(command.Id, cancellationToken);

    public Task<bool> Handle(UnpublishAdminProductCommand command, CancellationToken cancellationToken) =>
        service.UnpublishProductAsync(command.Id, cancellationToken);

    public Task<bool> Handle(ToggleAdminProductActivationCommand command, CancellationToken cancellationToken) =>
        service.ToggleProductActivationAsync(command.Id, command.IsActive, cancellationToken);

    public Task<AdminProductDto?> Handle(DuplicateAdminProductCommand command, CancellationToken cancellationToken) =>
        service.DuplicateProductAsync(command.Id, cancellationToken);

    public Task<IReadOnlyList<ProductImageDto>> Handle(UploadAdminProductImagesCommand command, CancellationToken cancellationToken) =>
        service.UploadImagesAsync(command.Id, command.Files, cancellationToken);

    public Task<bool> Handle(DeleteAdminProductImageCommand command, CancellationToken cancellationToken) =>
        service.DeleteImageAsync(command.Id, command.ImageId, cancellationToken);

    public Task<bool> Handle(ReorderAdminProductImagesCommand command, CancellationToken cancellationToken) =>
        service.ReorderImagesAsync(command.Id, command.ImageIds, cancellationToken);
}

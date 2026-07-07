using MediatR;
using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Products;

// Queries
public sealed record GetProductsQuery(ProductSearchRequest Request) : IRequest<PagedResult<ProductDto>>;
public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductDto?>;
public sealed record GetFeaturedProductsQuery(int Count) : IRequest<IReadOnlyList<ProductDto>>;
public sealed record GetBestSellerProductsQuery(int Count) : IRequest<IReadOnlyList<ProductDto>>;
public sealed record GetNewArrivalProductsQuery(int Count) : IRequest<IReadOnlyList<ProductDto>>;
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
public sealed record GetAttributesQuery : IRequest<IReadOnlyList<ProductAttributeDto>>;

// Commands
public sealed record CreateProductCommand(UpsertProductRequest Request) : IRequest<ProductDto>;
public sealed record UpdateProductCommand(Guid Id, UpsertProductRequest Request) : IRequest<ProductDto?>;
public sealed record DeleteProductCommand(Guid Id) : IRequest<bool>;
public sealed record RestoreProductCommand(Guid Id) : IRequest<bool>;
public sealed record ActivateProductCommand(Guid Id) : IRequest<bool>;
public sealed record DeactivateProductCommand(Guid Id) : IRequest<bool>;
public sealed record PublishProductCommand(Guid Id) : IRequest<bool>;
public sealed record UnpublishProductCommand(Guid Id) : IRequest<bool>;
public sealed record DuplicateProductCommand(Guid Id) : IRequest<ProductDto?>;

// Images
public sealed record UploadProductImageCommand(Guid ProductId, IFormFile File, bool IsPrimary) : IRequest<ProductImageDto?>;
public sealed record DeleteProductImageCommand(Guid ProductId, Guid ImageId) : IRequest<bool>;
public sealed record ReorderProductImagesCommand(Guid ProductId, List<ImageReorderRequest> Requests) : IRequest<bool>;

// Categories & Attributes
public sealed record CreateCategoryCommand(UpsertCategoryRequest Request) : IRequest<CategoryDto>;
public sealed record CreateAttributeCommand(UpsertProductAttributeRequest Request) : IRequest<ProductAttributeDto>;

public sealed class ProductHandlers(IProductService products) :
    IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>,
    IRequestHandler<GetProductByIdQuery, ProductDto?>,
    IRequestHandler<GetProductBySlugQuery, ProductDto?>,
    IRequestHandler<GetFeaturedProductsQuery, IReadOnlyList<ProductDto>>,
    IRequestHandler<GetBestSellerProductsQuery, IReadOnlyList<ProductDto>>,
    IRequestHandler<GetNewArrivalProductsQuery, IReadOnlyList<ProductDto>>,
    IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>,
    IRequestHandler<GetAttributesQuery, IReadOnlyList<ProductAttributeDto>>,
    
    IRequestHandler<CreateProductCommand, ProductDto>,
    IRequestHandler<UpdateProductCommand, ProductDto?>,
    IRequestHandler<DeleteProductCommand, bool>,
    IRequestHandler<RestoreProductCommand, bool>,
    IRequestHandler<ActivateProductCommand, bool>,
    IRequestHandler<DeactivateProductCommand, bool>,
    IRequestHandler<PublishProductCommand, bool>,
    IRequestHandler<UnpublishProductCommand, bool>,
    IRequestHandler<DuplicateProductCommand, ProductDto?>,
    
    IRequestHandler<UploadProductImageCommand, ProductImageDto?>,
    IRequestHandler<DeleteProductImageCommand, bool>,
    IRequestHandler<ReorderProductImagesCommand, bool>,
    
    IRequestHandler<CreateCategoryCommand, CategoryDto>,
    IRequestHandler<CreateAttributeCommand, ProductAttributeDto>
{
    public Task<PagedResult<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken) =>
        products.SearchAsync(query.Request, cancellationToken);

    public Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken) =>
        products.GetAsync(query.Id, cancellationToken);

    public Task<ProductDto?> Handle(GetProductBySlugQuery query, CancellationToken cancellationToken) =>
        products.GetBySlugAsync(query.Slug, cancellationToken);

    public Task<IReadOnlyList<ProductDto>> Handle(GetFeaturedProductsQuery query, CancellationToken cancellationToken) =>
        products.GetFeaturedAsync(query.Count, cancellationToken);

    public Task<IReadOnlyList<ProductDto>> Handle(GetBestSellerProductsQuery query, CancellationToken cancellationToken) =>
        products.GetBestSellersAsync(query.Count, cancellationToken);

    public Task<IReadOnlyList<ProductDto>> Handle(GetNewArrivalProductsQuery query, CancellationToken cancellationToken) =>
        products.GetNewArrivalsAsync(query.Count, cancellationToken);

    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken) =>
        products.GetCategoriesAsync(cancellationToken);

    public Task<IReadOnlyList<ProductAttributeDto>> Handle(GetAttributesQuery query, CancellationToken cancellationToken) =>
        products.GetAttributesAsync(cancellationToken);

    public Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken) =>
        products.CreateAsync(command.Request, cancellationToken);

    public Task<ProductDto?> Handle(UpdateProductCommand command, CancellationToken cancellationToken) =>
        products.UpdateAsync(command.Id, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken) =>
        products.DeleteAsync(command.Id, cancellationToken);

    public Task<bool> Handle(RestoreProductCommand command, CancellationToken cancellationToken) =>
        products.RestoreAsync(command.Id, cancellationToken);

    public Task<bool> Handle(ActivateProductCommand command, CancellationToken cancellationToken) =>
        products.ActivateAsync(command.Id, cancellationToken);

    public Task<bool> Handle(DeactivateProductCommand command, CancellationToken cancellationToken) =>
        products.DeactivateAsync(command.Id, cancellationToken);

    public Task<bool> Handle(PublishProductCommand command, CancellationToken cancellationToken) =>
        products.PublishAsync(command.Id, cancellationToken);

    public Task<bool> Handle(UnpublishProductCommand command, CancellationToken cancellationToken) =>
        products.UnpublishAsync(command.Id, cancellationToken);

    public Task<ProductDto?> Handle(DuplicateProductCommand command, CancellationToken cancellationToken) =>
        products.DuplicateAsync(command.Id, cancellationToken);

    public Task<ProductImageDto?> Handle(UploadProductImageCommand command, CancellationToken cancellationToken) =>
        products.UploadImageAsync(command.ProductId, command.File, command.IsPrimary, cancellationToken);

    public Task<bool> Handle(DeleteProductImageCommand command, CancellationToken cancellationToken) =>
        products.DeleteImageAsync(command.ProductId, command.ImageId, cancellationToken);

    public Task<bool> Handle(ReorderProductImagesCommand command, CancellationToken cancellationToken) =>
        products.ReorderImagesAsync(command.ProductId, command.Requests, cancellationToken);

    public Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken) =>
        products.CreateCategoryAsync(command.Request, cancellationToken);

    public Task<ProductAttributeDto> Handle(CreateAttributeCommand command, CancellationToken cancellationToken) =>
        products.CreateAttributeAsync(command.Request, cancellationToken);
}

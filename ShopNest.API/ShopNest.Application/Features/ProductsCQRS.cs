using MediatR;
using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Products;

public sealed record GetProductsQuery(ProductSearchRequest Request) : IRequest<PagedResult<ProductDto>>;
public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public sealed record CreateProductCommand(UpsertProductRequest Request) : IRequest<ProductDto>;
public sealed record UpdateProductCommand(Guid Id, UpsertProductRequest Request) : IRequest<ProductDto?>;
public sealed record DeleteProductCommand(Guid Id) : IRequest<bool>;
public sealed record UploadProductImageCommand(Guid ProductId, IFormFile File, bool IsPrimary) : IRequest<ProductImageDto?>;
public sealed record CreateCategoryCommand(UpsertCategoryRequest Request) : IRequest<CategoryDto>;

public sealed class ProductHandlers(IProductService products) :
    IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>,
    IRequestHandler<GetProductByIdQuery, ProductDto?>,
    IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>,
    IRequestHandler<CreateProductCommand, ProductDto>,
    IRequestHandler<UpdateProductCommand, ProductDto?>,
    IRequestHandler<DeleteProductCommand, bool>,
    IRequestHandler<UploadProductImageCommand, ProductImageDto?>,
    IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public Task<PagedResult<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken) =>
        products.SearchAsync(query.Request, cancellationToken);

    public Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken) =>
        products.GetAsync(query.Id, cancellationToken);

    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken) =>
        products.GetCategoriesAsync(cancellationToken);

    public Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken) =>
        products.CreateAsync(command.Request, cancellationToken);

    public Task<ProductDto?> Handle(UpdateProductCommand command, CancellationToken cancellationToken) =>
        products.UpdateAsync(command.Id, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken) =>
        products.DeleteAsync(command.Id, cancellationToken);

    public Task<ProductImageDto?> Handle(UploadProductImageCommand command, CancellationToken cancellationToken) =>
        products.UploadImageAsync(command.ProductId, command.File, command.IsPrimary, cancellationToken);

    public Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken) =>
        products.CreateCategoryAsync(command.Request, cancellationToken);
}

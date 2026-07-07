using MediatR;
using Microsoft.AspNetCore.Http;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Categories;

// Queries
public sealed record GetAdminCategoriesQuery(
    string? Search,
    bool? IsActive,
    bool? IsFeatured,
    Guid? ParentId,
    bool? IsDeleted,
    string? SortBy,
    bool Descending,
    int Page,
    int PageSize
) : IRequest<PagedResult<AdminCategoryDto>>;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<AdminCategoryDto?>;
public sealed record GetCategoryTreeQuery : IRequest<IReadOnlyList<CategoryNodeDto>>;
public sealed record GetParentCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

// Commands
public sealed record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<AdminCategoryDto>;
public sealed record UpdateCategoryCommand(Guid Id, UpdateCategoryRequest Request) : IRequest<AdminCategoryDto?>;
public sealed record DeleteCategoryCommand(Guid Id) : IRequest<bool>;
public sealed record ToggleCategoryActivationCommand(Guid Id, bool IsActive) : IRequest<bool>;
public sealed record RestoreCategoryCommand(Guid Id) : IRequest<bool>;
public sealed record UploadCategoryImageCommand(Guid Id, IFormFile File) : IRequest<string?>;
public sealed record UploadCategoryBannerCommand(Guid Id, IFormFile File) : IRequest<string?>;

// Handlers
public sealed class CategoryHandlers(ICategoryService categories) :
    IRequestHandler<GetAdminCategoriesQuery, PagedResult<AdminCategoryDto>>,
    IRequestHandler<GetCategoryByIdQuery, AdminCategoryDto?>,
    IRequestHandler<GetCategoryTreeQuery, IReadOnlyList<CategoryNodeDto>>,
    IRequestHandler<GetParentCategoriesQuery, IReadOnlyList<CategoryDto>>,
    IRequestHandler<CreateCategoryCommand, AdminCategoryDto>,
    IRequestHandler<UpdateCategoryCommand, AdminCategoryDto?>,
    IRequestHandler<DeleteCategoryCommand, bool>,
    IRequestHandler<ToggleCategoryActivationCommand, bool>,
    IRequestHandler<RestoreCategoryCommand, bool>,
    IRequestHandler<UploadCategoryImageCommand, string?>,
    IRequestHandler<UploadCategoryBannerCommand, string?>
{
    public Task<PagedResult<AdminCategoryDto>> Handle(GetAdminCategoriesQuery query, CancellationToken cancellationToken) =>
        categories.GetCategoriesAsync(
            query.Search, query.IsActive, query.IsFeatured, query.ParentId, query.IsDeleted,
            query.SortBy, query.Descending, query.Page, query.PageSize, cancellationToken);

    public Task<AdminCategoryDto?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken) =>
        categories.GetCategoryByIdAsync(query.Id, cancellationToken);

    public Task<IReadOnlyList<CategoryNodeDto>> Handle(GetCategoryTreeQuery query, CancellationToken cancellationToken) =>
        categories.GetCategoryTreeAsync(cancellationToken);

    public Task<IReadOnlyList<CategoryDto>> Handle(GetParentCategoriesQuery query, CancellationToken cancellationToken) =>
        categories.GetParentCategoriesAsync(cancellationToken);

    public Task<AdminCategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken) =>
        categories.CreateCategoryAsync(command.Request, cancellationToken);

    public Task<AdminCategoryDto?> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken) =>
        categories.UpdateCategoryAsync(command.Id, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken) =>
        categories.DeleteCategoryAsync(command.Id, cancellationToken);

    public Task<bool> Handle(ToggleCategoryActivationCommand command, CancellationToken cancellationToken) =>
        categories.ToggleCategoryActivationAsync(command.Id, command.IsActive, cancellationToken);

    public Task<bool> Handle(RestoreCategoryCommand command, CancellationToken cancellationToken) =>
        categories.RestoreCategoryAsync(command.Id, cancellationToken);

    public Task<string?> Handle(UploadCategoryImageCommand command, CancellationToken cancellationToken) =>
        categories.UploadImageAsync(command.Id, command.File, cancellationToken);

    public Task<string?> Handle(UploadCategoryBannerCommand command, CancellationToken cancellationToken) =>
        categories.UploadBannerAsync(command.Id, command.File, cancellationToken);
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Products;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminProductDto>>>> Get(
        [FromQuery] string? query,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? stockStatus,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isPublished,
        [FromQuery] bool? isFeatured,
        [FromQuery] bool? isNewArrival,
        [FromQuery] bool? isBestSeller,
        [FromQuery] bool? isDeleted,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Enforce customer constraints: Non-admins can only see active, published, non-deleted products
        var isAdmin = User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"));
        if (!isAdmin)
        {
            isActive = true;
            isPublished = true;
            isDeleted = false;
        }

        var result = await mediator.Send(new GetAdminProductsQuery(
            query, categoryId, brandId, minPrice, maxPrice, stockStatus,
            isActive, isPublished, isFeatured, isNewArrival, isBestSeller,
            isDeleted, sortBy, descending, page, pageSize), cancellationToken);

        return Ok(ApiResponse<PagedResult<AdminProductDto>>.SuccessResult(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetAdminProductByIdQuery(id), cancellationToken);
        if (product is null) return NotFound(ApiResponse.FailureResult("Product not found."));

        var isAdmin = User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"));
        if (!isAdmin && (!product.IsActive || !product.IsPublished || product.IsDeleted))
        {
            return Forbid();
        }

        return Ok(ApiResponse<AdminProductDto>.SuccessResult(product));
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetAdminProductBySlugQuery(slug), cancellationToken);
        if (product is null) return NotFound(ApiResponse.FailureResult("Product not found."));

        var isAdmin = User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"));
        if (!isAdmin && (!product.IsActive || !product.IsPublished || product.IsDeleted))
        {
            return Forbid();
        }

        return Ok(ApiResponse<AdminProductDto>.SuccessResult(product));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminProductDto>>>> GetFeatured([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminFeaturedProductsQuery(count), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminProductDto>>.SuccessResult(result));
    }

    [HttpGet("bestseller")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminProductDto>>>> GetBestSellers([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminBestSellerProductsQuery(count), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminProductDto>>.SuccessResult(result));
    }

    [HttpGet("new-arrival")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminProductDto>>>> GetNewArrivals([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminNewArrivalProductsQuery(count), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminProductDto>>.SuccessResult(result));
    }

    // ==========================================
    // WRITE ENDPOINTS (Admin / SuperAdmin Only)
    // ==========================================
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateAdminProductCommand(request), cancellationToken);
        return Ok(ApiResponse<AdminProductDto>.SuccessResult(result, "Product created successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminProductCommand(id, request), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<AdminProductDto>.SuccessResult(result, "Product updated successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAdminProductCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product soft-deleted successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestoreAdminProductCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product restored successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}/publish")]
    public async Task<ActionResult<ApiResponse<bool>>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PublishAdminProductCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product published successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}/unpublish")]
    public async Task<ActionResult<ApiResponse<bool>>> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnpublishAdminProductCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product unpublished successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleAdminProductActivationCommand(id, true), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product activated successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<bool>>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleAdminProductActivationCommand(id, false), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Product deactivated successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> Duplicate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DuplicateAdminProductCommand(id), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Product not found."));
        return Ok(ApiResponse<AdminProductDto>.SuccessResult(result, "Product duplicated successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{id:guid}/images")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductImageDto>>>> UploadImages(Guid id, List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0) return BadRequest(ApiResponse.FailureResult("No files uploaded."));
        var result = await mediator.Send(new UploadAdminProductImagesCommand(id, files), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProductImageDto>>.SuccessResult(result, "Images uploaded successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAdminProductImageCommand(id, imageId), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Image not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Image deleted successfully."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:guid}/images/reorder")]
    public async Task<ActionResult<ApiResponse<bool>>> ReorderImages(Guid id, [FromBody] List<Guid> imageIds, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReorderAdminProductImagesCommand(id, imageIds), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Reordering failed."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Images reordered successfully."));
    }
}

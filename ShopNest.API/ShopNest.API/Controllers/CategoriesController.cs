using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Categories;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminCategoryDto>>>> Get(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isFeatured,
        [FromQuery] Guid? parentId,
        [FromQuery] bool? isDeleted,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Enforce visibility constraint: Customers can only query active and non-deleted categories
        var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        if (!isAdmin)
        {
            isActive = true;
            isDeleted = false;
        }

        var result = await mediator.Send(new GetAdminCategoriesQuery(
            search, isActive, isFeatured, parentId, isDeleted, sortBy, descending, page, pageSize), cancellationToken);

        return Ok(ApiResponse<PagedResult<AdminCategoryDto>>.SuccessResult(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminCategoryDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Category not found."));
        
        var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        if (!isAdmin && (!result.IsActive || result.IsDeleted))
        {
            return Forbid();
        }

        return Ok(ApiResponse<AdminCategoryDto>.SuccessResult(result));
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryNodeDto>>>> GetTree(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCategoryTreeQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryNodeDto>>.SuccessResult(result));
    }

    [HttpGet("parents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetParents(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetParentCategoriesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResult(result));
    }

    // ==========================================
    // WRITE ENDPOINTS (Admin Only)
    // ==========================================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminCategoryDto>>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCategoryCommand(request), cancellationToken);
        return Ok(ApiResponse<AdminCategoryDto>.SuccessResult(result, "Category created successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminCategoryDto>>> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCategoryCommand(id, request), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Category not found."));
        return Ok(ApiResponse<AdminCategoryDto>.SuccessResult(result, "Category updated successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Category not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Category soft-deleted successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleCategoryActivationCommand(id, true), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Category not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Category activated successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<bool>>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleCategoryActivationCommand(id, false), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Category not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Category deactivated successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestoreCategoryCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Category not found or not deleted."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Category restored successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse.FailureResult("No file uploaded."));
        var result = await mediator.Send(new UploadCategoryImageCommand(id, file), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Category not found."));
        return Ok(ApiResponse<string>.SuccessResult(result, "Image uploaded successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/banner")]
    public async Task<ActionResult<ApiResponse<string>>> UploadBanner(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse.FailureResult("No file uploaded."));
        var result = await mediator.Send(new UploadCategoryBannerCommand(id, file), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Category not found."));
        return Ok(ApiResponse<string>.SuccessResult(result, "Banner image uploaded successfully."));
    }
}

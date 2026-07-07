using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features.Admin;

namespace ShopNest.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/management")]
public sealed class AdminManagementController(IMediator mediator) : ControllerBase
{
    // ==========================================
    // USER ENDPOINTS
    // ==========================================
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserDto>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminUsersQuery(search, role, sortBy, descending, page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.SuccessResult(result));
    }

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminUserByIdQuery(id), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<AdminUserDto>.SuccessResult(result));
    }

    [HttpPost("users")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> CreateUser(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateAdminUserCommand(request), cancellationToken);
        return Ok(ApiResponse<AdminUserDto>.SuccessResult(result, "User created successfully."));
    }

    [HttpPut("users/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateUser(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminUserCommand(id, request), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<AdminUserDto>.SuccessResult(result, "User details updated successfully."));
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAdminUserCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "User soft-deleted successfully."));
    }

    [HttpPut("users/{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleUserActivationCommand(id, true), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "User activated successfully."));
    }

    [HttpPut("users/{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleUserActivationCommand(id, false), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "User deactivated successfully."));
    }

    [HttpPut("users/{id:guid}/lock")]
    public async Task<ActionResult<ApiResponse<bool>>> LockUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleUserLockCommand(id, true), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "User account locked successfully."));
    }

    [HttpPut("users/{id:guid}/unlock")]
    public async Task<ActionResult<ApiResponse<bool>>> UnlockUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleUserLockCommand(id, false), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "User account unlocked successfully."));
    }

    [HttpPut("users/{id:guid}/reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(Guid id, AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AdminResetPasswordCommand(id, request), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("User not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "User password reset successfully."));
    }

    // ==========================================
    // ROLE ENDPOINTS
    // ==========================================
    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminRoleDto>>>> GetRoles(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminRolesQuery(search, page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminRoleDto>>.SuccessResult(result));
    }

    [HttpGet("roles/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminRoleDto>>> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminRoleByIdQuery(id), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Role not found."));
        return Ok(ApiResponse<AdminRoleDto>.SuccessResult(result));
    }

    [HttpPost("roles")]
    public async Task<ActionResult<ApiResponse<AdminRoleDto>>> CreateRole(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateAdminRoleCommand(request), cancellationToken);
        return Ok(ApiResponse<AdminRoleDto>.SuccessResult(result, "Role created successfully."));
    }

    [HttpPut("roles/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminRoleDto>>> UpdateRole(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminRoleCommand(id, request), cancellationToken);
        if (result is null) return NotFound(ApiResponse.FailureResult("Role not found."));
        return Ok(ApiResponse<AdminRoleDto>.SuccessResult(result, "Role updated successfully."));
    }

    [HttpDelete("roles/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAdminRoleCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Role not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Role soft-deleted successfully."));
    }

    [HttpPut("roles/{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleRoleActivationCommand(id, true), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Role not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Role activated successfully."));
    }

    [HttpPut("roles/{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleRoleActivationCommand(id, false), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Role not found."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Role deactivated successfully."));
    }

    [HttpPut("roles/{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestoreRoleCommand(id), cancellationToken);
        if (!result) return NotFound(ApiResponse.FailureResult("Role not found or not soft-deleted."));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Role restored successfully."));
    }

    [HttpPut("users/{userId:guid}/assign-role/{roleId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignUserRoleCommand(userId, roleId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Role assigned successfully."));
    }

    [HttpPut("users/{userId:guid}/remove-role/{roleId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveUserRoleCommand(userId, roleId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Role removed successfully."));
    }
}

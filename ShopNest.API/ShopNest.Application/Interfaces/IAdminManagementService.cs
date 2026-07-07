using ShopNest.Application.Common;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IAdminManagementService
{
    // User Administration Methods
    Task<PagedResult<AdminUserDto>> GetUsersAsync(string? search, string? filterRole, string? sortBy, bool descending, int page, int pageSize, CancellationToken cancellationToken);
    Task<AdminUserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminUserDto> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken);
    Task<AdminUserDto?> UpdateUserAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ToggleUserActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<bool> ToggleUserLockAsync(Guid id, bool isLocked, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(Guid id, AdminResetPasswordRequest request, CancellationToken cancellationToken);
    
    // Role Administration Methods
    Task<PagedResult<AdminRoleDto>> GetRolesAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<AdminRoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminRoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task<AdminRoleDto?> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ToggleRoleActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<bool> RestoreRoleAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AssignUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
    Task<bool> RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
}

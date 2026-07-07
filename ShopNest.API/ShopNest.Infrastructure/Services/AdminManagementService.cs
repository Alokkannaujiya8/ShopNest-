using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class AdminManagementService(
    ShopNestDbContext db,
    IMapper mapper) : IAdminManagementService
{
    // ==========================================
    // USER MANAGEMENT
    // ==========================================
    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(
        string? search,
        string? filterRole,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking();

        // 1. Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => 
                x.FullName.ToLower().Contains(term) ||
                x.Email.ToLower().Contains(term) ||
                x.MobileNumber.Contains(term)
            );
        }

        // 2. Role filter
        if (!string.IsNullOrWhiteSpace(filterRole) && Enum.TryParse<UserRole>(filterRole, true, out var roleEnum))
        {
            query = query.Where(x => x.Role == roleEnum);
        }

        // 3. Sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "fullname" => descending ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => descending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "role" => descending ? query.OrderByDescending(x => x.Role) : query.OrderBy(x => x.Role),
            "createdat" => descending ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.CreatedAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var mapped = mapper.Map<IReadOnlyList<AdminUserDto>>(items);
        return new PagedResult<AdminUserDto>(mapped, page, pageSize, totalCount);
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return mapper.Map<AdminUserDto>(user);
    }

    public async Task<AdminUserDto> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        
        var alreadyExists = await db.Users.AnyAsync(x => x.Email == email, cancellationToken);
        if (alreadyExists)
        {
            throw new InvalidOperationException("Email address is already in use.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var roleEnum))
        {
            throw new InvalidOperationException("Invalid user role.");
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            MobileNumber = request.MobileNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = roleEnum,
            IsEmailVerified = true, // Created by admin: pre-verified
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        // Map to DB Role table if Role exists
        var dbRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == request.Role, cancellationToken);
        if (dbRole is not null)
        {
            db.UserRoles.Add(new AppUserRole { UserId = user.Id, RoleId = dbRole.Id });
            await db.SaveChangesAsync(cancellationToken);
        }

        return mapper.Map<AdminUserDto>(user);
    }

    public async Task<AdminUserDto?> UpdateUserAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return null;

        if (!Enum.TryParse<UserRole>(request.Role, true, out var roleEnum))
        {
            throw new InvalidOperationException("Invalid user role.");
        }

        user.FullName = request.FullName.Trim();
        user.MobileNumber = request.MobileNumber.Trim();
        user.Role = roleEnum;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        // Synchronize in DB UserRoles mapping table
        var dbRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == request.Role, cancellationToken);
        if (dbRole is not null)
        {
            // Remove previous roles
            var oldRoles = await db.UserRoles.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);
            db.UserRoles.RemoveRange(oldRoles);

            db.UserRoles.Add(new AppUserRole { UserId = user.Id, RoleId = dbRole.Id });
        }

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<AdminUserDto>(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return false;

        db.Users.Remove(user); // Soft-delete is intercepted in SaveChangesAsync
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleUserActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return false;

        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleUserLockAsync(Guid id, bool isLocked, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return false;

        user.IsLocked = isLocked;
        user.LockoutEndUtc = isLocked ? DateTime.UtcNow.AddYears(100) : null; // Locks out user
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid id, AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ForcePasswordChange = request.ForcePasswordChange;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ==========================================
    // ROLE MANAGEMENT
    // ==========================================
    public async Task<PagedResult<AdminRoleDto>> GetRolesAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => 
                x.Name.ToLower().Contains(term) ||
                x.DisplayName.ToLower().Contains(term)
            );
        }

        query = query.OrderBy(x => x.DisplayName);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var mapped = mapper.Map<IReadOnlyList<AdminRoleDto>>(items);
        return new PagedResult<AdminRoleDto>(mapped, page, pageSize, totalCount);
    }

    public async Task<AdminRoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await db.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return mapper.Map<AdminRoleDto>(role);
    }

    public async Task<AdminRoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var code = request.Name.Trim().Replace(" ", "");
        var exists = await db.Roles.AnyAsync(x => x.Name == code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Role code name already exists.");
        }

        var role = new Role
        {
            Name = code,
            DisplayName = request.DisplayName.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<AdminRoleDto>(role);
    }

    public async Task<AdminRoleDto?> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null) return null;

        role.DisplayName = request.DisplayName.Trim();
        role.Description = request.Description?.Trim();
        role.IsActive = request.IsActive;
        role.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<AdminRoleDto>(role);
    }

    public async Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null) return false;

        // Prevent deleting seeded roles
        var systemRoles = new[] { "Admin", "Customer", "Seller", "SuperAdmin" };
        if (systemRoles.Contains(role.Name))
        {
            throw new InvalidOperationException("System predefined roles cannot be deleted.");
        }

        db.Roles.Remove(role); // Soft delete
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleRoleActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null) return false;

        role.IsActive = isActive;
        role.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        // To query soft-deleted records, temporarily disable global query filters or retrieve manually
        var role = await db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (role is null || !role.IsDeleted) return false;

        role.IsDeleted = false;
        role.DeletedAtUtc = null;
        role.DeletedBy = null;
        role.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var exists = await db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
        if (exists) return true;

        var assignment = new AppUserRole { UserId = userId, RoleId = roleId };
        db.UserRoles.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var assignment = await db.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
        if (assignment is null) return true;

        db.UserRoles.Remove(assignment);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

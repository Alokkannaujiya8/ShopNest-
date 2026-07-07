using MediatR;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features.Admin;

// User Queries
public sealed record GetAdminUsersQuery(string? Search, string? FilterRole, string? SortBy, bool Descending, int Page, int PageSize) : IRequest<PagedResult<AdminUserDto>>;
public sealed record GetAdminUserByIdQuery(Guid Id) : IRequest<AdminUserDto?>;

// User Commands
public sealed record CreateAdminUserCommand(CreateAdminUserRequest Request) : IRequest<AdminUserDto>;
public sealed record UpdateAdminUserCommand(Guid Id, UpdateAdminUserRequest Request) : IRequest<AdminUserDto?>;
public sealed record DeleteAdminUserCommand(Guid Id) : IRequest<bool>;
public sealed record ToggleUserActivationCommand(Guid Id, bool IsActive) : IRequest<bool>;
public sealed record ToggleUserLockCommand(Guid Id, bool IsLocked) : IRequest<bool>;
public sealed record AdminResetPasswordCommand(Guid Id, AdminResetPasswordRequest Request) : IRequest<bool>;

// Role Queries
public sealed record GetAdminRolesQuery(string? Search, int Page, int PageSize) : IRequest<PagedResult<AdminRoleDto>>;
public sealed record GetAdminRoleByIdQuery(Guid Id) : IRequest<AdminRoleDto?>;

// Role Commands
public sealed record CreateAdminRoleCommand(CreateRoleRequest Request) : IRequest<AdminRoleDto>;
public sealed record UpdateAdminRoleCommand(Guid Id, UpdateRoleRequest Request) : IRequest<AdminRoleDto?>;
public sealed record DeleteAdminRoleCommand(Guid Id) : IRequest<bool>;
public sealed record ToggleRoleActivationCommand(Guid Id, bool IsActive) : IRequest<bool>;
public sealed record RestoreRoleCommand(Guid Id) : IRequest<bool>;
public sealed record AssignUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<bool>;
public sealed record RemoveUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<bool>;

// Handlers mapping directly to IAdminManagementService
public sealed class AdminManagementHandlers(IAdminManagementService adminService) :
    IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserDto>>,
    IRequestHandler<GetAdminUserByIdQuery, AdminUserDto?>,
    IRequestHandler<CreateAdminUserCommand, AdminUserDto>,
    IRequestHandler<UpdateAdminUserCommand, AdminUserDto?>,
    IRequestHandler<DeleteAdminUserCommand, bool>,
    IRequestHandler<ToggleUserActivationCommand, bool>,
    IRequestHandler<ToggleUserLockCommand, bool>,
    IRequestHandler<AdminResetPasswordCommand, bool>,
    IRequestHandler<GetAdminRolesQuery, PagedResult<AdminRoleDto>>,
    IRequestHandler<GetAdminRoleByIdQuery, AdminRoleDto?>,
    IRequestHandler<CreateAdminRoleCommand, AdminRoleDto>,
    IRequestHandler<UpdateAdminRoleCommand, AdminRoleDto?>,
    IRequestHandler<DeleteAdminRoleCommand, bool>,
    IRequestHandler<ToggleRoleActivationCommand, bool>,
    IRequestHandler<RestoreRoleCommand, bool>,
    IRequestHandler<AssignUserRoleCommand, bool>,
    IRequestHandler<RemoveUserRoleCommand, bool>
{
    public Task<PagedResult<AdminUserDto>> Handle(GetAdminUsersQuery query, CancellationToken cancellationToken) =>
        adminService.GetUsersAsync(query.Search, query.FilterRole, query.SortBy, query.Descending, query.Page, query.PageSize, cancellationToken);

    public Task<AdminUserDto?> Handle(GetAdminUserByIdQuery query, CancellationToken cancellationToken) =>
        adminService.GetUserByIdAsync(query.Id, cancellationToken);

    public Task<AdminUserDto> Handle(CreateAdminUserCommand command, CancellationToken cancellationToken) =>
        adminService.CreateUserAsync(command.Request, cancellationToken);

    public Task<AdminUserDto?> Handle(UpdateAdminUserCommand command, CancellationToken cancellationToken) =>
        adminService.UpdateUserAsync(command.Id, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteAdminUserCommand command, CancellationToken cancellationToken) =>
        adminService.DeleteUserAsync(command.Id, cancellationToken);

    public Task<bool> Handle(ToggleUserActivationCommand command, CancellationToken cancellationToken) =>
        adminService.ToggleUserActivationAsync(command.Id, command.IsActive, cancellationToken);

    public Task<bool> Handle(ToggleUserLockCommand command, CancellationToken cancellationToken) =>
        adminService.ToggleUserLockAsync(command.Id, command.IsLocked, cancellationToken);

    public Task<bool> Handle(AdminResetPasswordCommand command, CancellationToken cancellationToken) =>
        adminService.ResetPasswordAsync(command.Id, command.Request, cancellationToken);

    public Task<PagedResult<AdminRoleDto>> Handle(GetAdminRolesQuery query, CancellationToken cancellationToken) =>
        adminService.GetRolesAsync(query.Search, query.Page, query.PageSize, cancellationToken);

    public Task<AdminRoleDto?> Handle(GetAdminRoleByIdQuery query, CancellationToken cancellationToken) =>
        adminService.GetRoleByIdAsync(query.Id, cancellationToken);

    public Task<AdminRoleDto> Handle(CreateAdminRoleCommand command, CancellationToken cancellationToken) =>
        adminService.CreateRoleAsync(command.Request, cancellationToken);

    public Task<AdminRoleDto?> Handle(UpdateAdminRoleCommand command, CancellationToken cancellationToken) =>
        adminService.UpdateRoleAsync(command.Id, command.Request, cancellationToken);

    public Task<bool> Handle(DeleteAdminRoleCommand command, CancellationToken cancellationToken) =>
        adminService.DeleteRoleAsync(command.Id, cancellationToken);

    public Task<bool> Handle(ToggleRoleActivationCommand command, CancellationToken cancellationToken) =>
        adminService.ToggleRoleActivationAsync(command.Id, command.IsActive, cancellationToken);

    public Task<bool> Handle(RestoreRoleCommand command, CancellationToken cancellationToken) =>
        adminService.RestoreRoleAsync(command.Id, cancellationToken);

    public Task<bool> Handle(AssignUserRoleCommand command, CancellationToken cancellationToken) =>
        adminService.AssignUserRoleAsync(command.UserId, command.RoleId, cancellationToken);

    public Task<bool> Handle(RemoveUserRoleCommand command, CancellationToken cancellationToken) =>
        adminService.RemoveUserRoleAsync(command.UserId, command.RoleId, cancellationToken);
}

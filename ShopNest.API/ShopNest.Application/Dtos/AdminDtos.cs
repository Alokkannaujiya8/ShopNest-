namespace ShopNest.Application.Dtos;

// Original Dashboard & Connection stats records
public sealed record DashboardStatsDto(int Customers, int Products, int LowStockProducts, int Orders, decimal Revenue, int PendingOrders);
public sealed record InventoryUpdateRequest(int StockQuantity, bool IsActive);

public sealed record ConnectionDetails(
    string ConnectionId,
    Guid? UserId,
    string FullName,
    string Role,
    DateTime LoginTime,
    DateTime LastActivityTime
);

public sealed record OnlineDashboardStatsDto(
    int TotalOnlineUsers,
    int ActiveConnections,
    int LoggedInCustomers,
    int LoggedOutCustomers,
    List<ConnectionDetails> ActiveUsersList
);

// New Role & User Management records
public sealed record AdminUserDto(
    Guid Id,
    string FullName,
    string Email,
    string MobileNumber,
    string Role,
    bool IsActive,
    bool IsLocked,
    DateTime? LockoutEndUtc,
    DateTime? LastLoginUtc,
    int LoginCount,
    bool EmailVerified,
    bool MobileVerified,
    DateTime CreatedAtUtc
);

public sealed record AdminRoleDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc
);

public sealed record CreateAdminUserRequest(
    string FullName,
    string Email,
    string MobileNumber,
    string Password,
    string Role
);

public sealed record UpdateAdminUserRequest(
    string FullName,
    string MobileNumber,
    string Role,
    bool IsActive
);

public sealed record AdminRoleAssignmentRequest(
    Guid UserId,
    Guid RoleId
);

public sealed record CreateRoleRequest(
    string Name,
    string DisplayName,
    string? Description
);

public sealed record UpdateRoleRequest(
    string DisplayName,
    string? Description,
    bool IsActive
);

public sealed record AdminResetPasswordRequest(
    string NewPassword,
    string ConfirmPassword,
    bool ForcePasswordChange
);

namespace ShopNest.Application.Dtos;

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

using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;

namespace ShopNest.Infrastructure.Hubs;

public sealed class OrderHub(
    IUserConnectionManager connectionManager,
    IServiceProvider serviceProvider,
    ILogger<OrderHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var identity = Context.User?.Identity;
        string fullName = "Guest User";
        string role = "Guest";
        Guid? userId = null;

        if (identity is not null && identity.IsAuthenticated)
        {
            var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var id))
            {
                userId = id;
            }
            fullName = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Authenticated User";
            role = Context.User?.FindFirstValue(ClaimTypes.Role) ?? "Customer";

            // Add user to their private channel
            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.Value.ToString());
            }

            // Add user to their role group (e.g. "Admin")
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }

        connectionManager.AddConnection(Context.ConnectionId, userId, fullName, role);
        logger.LogInformation("Connected: {ConnectionId}, User: {FullName} ({Role})", Context.ConnectionId, fullName, role);

        // Broadcast stats and connection event to Admins
        await BroadcastOnlineStatsAsync();
        
        if (role == "Admin")
        {
            // If the connecting user is an Admin, send them the current stats immediately
            var stats = await GetStatsAsync();
            await Clients.Caller.SendAsync("onlineStatsUpdated", stats);
        }

        if (userId.HasValue)
        {
            await Clients.Group("Admin").SendAsync("notificationReceived", new
            {
                type = "UserLoggedIn",
                message = $"{fullName} logged in.",
                timestamp = DateTime.UtcNow
            });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var activeConnections = connectionManager.GetActiveConnections();
        var conn = activeConnections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

        connectionManager.RemoveConnection(Context.ConnectionId);
        logger.LogInformation("Disconnected: {ConnectionId}", Context.ConnectionId);

        await BroadcastOnlineStatsAsync();

        if (conn is not null && conn.UserId.HasValue)
        {
            // Only send log out event if they don't have other active connections
            var stillConnected = connectionManager.GetActiveConnections().Any(x => x.UserId == conn.UserId);
            if (!stillConnected)
            {
                await Clients.Group("Admin").SendAsync("notificationReceived", new
                {
                    type = "UserLoggedOut",
                    message = $"{conn.FullName} logged out.",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Heartbeat()
    {
        connectionManager.UpdateActivity(Context.ConnectionId);
        await BroadcastOnlineStatsAsync();
    }

    private async Task BroadcastOnlineStatsAsync()
    {
        var stats = await GetStatsAsync();
        await Clients.Group("Admin").SendAsync("onlineStatsUpdated", stats);
    }

    private async Task<object> GetStatsAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var totalCustomers = await uow.Repository<AppUser>().Query().CountAsync(x => x.Role == UserRole.Customer);
        return connectionManager.GetOnlineDashboardStats(totalCustomers);
    }
}

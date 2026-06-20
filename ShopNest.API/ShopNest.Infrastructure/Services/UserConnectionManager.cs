using System.Collections.Concurrent;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services;

public sealed class UserConnectionManager : IUserConnectionManager
{
    private readonly ConcurrentDictionary<string, ConnectionDetails> _connections = new();

    public void AddConnection(string connectionId, Guid? userId, string fullName, string role)
    {
        var now = DateTime.UtcNow;
        var details = new ConnectionDetails(connectionId, userId, fullName, role, now, now);
        _connections[connectionId] = details;
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public void UpdateActivity(string connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var details))
        {
            _connections[connectionId] = details with { LastActivityTime = DateTime.UtcNow };
        }
    }

    public IReadOnlyList<ConnectionDetails> GetActiveConnections()
    {
        return _connections.Values.ToList();
    }

    public OnlineDashboardStatsDto GetOnlineDashboardStats(int totalCustomersCount)
    {
        var activeList = _connections.Values.ToList();
        var totalActive = activeList.Count;

        // Distinct authenticated users
        var onlineUsers = activeList
            .Where(x => x.UserId.HasValue)
            .Select(x => x.UserId!.Value)
            .Distinct()
            .Count();

        // Distinct customers online
        var onlineCustomers = activeList
            .Where(x => x.UserId.HasValue && x.Role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.UserId!.Value)
            .Distinct()
            .Count();

        var offlineCustomers = Math.Max(0, totalCustomersCount - onlineCustomers);

        return new OnlineDashboardStatsDto(
            onlineUsers,
            totalActive,
            onlineCustomers,
            offlineCustomers,
            activeList
        );
    }
}

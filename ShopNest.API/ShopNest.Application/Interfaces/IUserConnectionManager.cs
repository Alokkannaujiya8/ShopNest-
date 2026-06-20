using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IUserConnectionManager
{
    void AddConnection(string connectionId, Guid? userId, string fullName, string role);
    void RemoveConnection(string connectionId);
    void UpdateActivity(string connectionId);
    IReadOnlyList<ConnectionDetails> GetActiveConnections();
    OnlineDashboardStatsDto GetOnlineDashboardStats(int totalCustomersCount);
}

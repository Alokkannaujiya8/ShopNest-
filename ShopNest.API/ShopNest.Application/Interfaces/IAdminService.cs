using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken);
    Task<bool> UpdateInventoryAsync(Guid productId, InventoryUpdateRequest request, CancellationToken cancellationToken);
}

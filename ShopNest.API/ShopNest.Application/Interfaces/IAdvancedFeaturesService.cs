using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;

namespace ShopNest.Application.Interfaces;

public sealed record GlobalSearchResultDto(
    List<ProductDto> Products,
    List<CategoryDto> Categories,
    List<string> Brands,
    List<string> CouponCodes
);

public interface IAdvancedFeaturesService
{
    Task<GlobalSearchResultDto> GlobalSearchAsync(string queryText, int limit, CancellationToken cancellationToken);
    Task<List<ProductCompareResultDto>> CompareProductsAsync(List<Guid> productIds, CancellationToken cancellationToken);
    Task<List<TimelineEventDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> RestoreRecordAsync(string entityName, Guid id, CancellationToken cancellationToken);
}

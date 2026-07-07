using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

// 1. Product Comparison Models
public sealed record ProductCompareResultDto(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    string Brand,
    string Category,
    double Rating,
    int ReviewsCount,
    string StockStatus,
    decimal Weight,
    string Dimensions, // Length x Width x Height
    List<string> Variants
);

public sealed record GetProductComparisonQuery(List<Guid> ProductIds) : IRequest<List<ProductCompareResultDto>>;

public sealed class GetProductComparisonQueryHandler : IRequestHandler<GetProductComparisonQuery, List<ProductCompareResultDto>>
{
    private readonly IAdvancedFeaturesService _advancedFeaturesService;

    public GetProductComparisonQueryHandler(IAdvancedFeaturesService advancedFeaturesService)
    {
        _advancedFeaturesService = advancedFeaturesService;
    }

    public Task<List<ProductCompareResultDto>> Handle(GetProductComparisonQuery request, CancellationToken cancellationToken)
    {
        return _advancedFeaturesService.CompareProductsAsync(request.ProductIds, cancellationToken);
    }
}

// 2. Timeline Models
public sealed record TimelineEventDto(
    string Title,
    string Description,
    DateTime Timestamp,
    bool IsCompleted
);

public sealed record GetOrderTimelineQuery(Guid OrderId) : IRequest<List<TimelineEventDto>>;

public sealed class GetOrderTimelineQueryHandler : IRequestHandler<GetOrderTimelineQuery, List<TimelineEventDto>>
{
    private readonly IAdvancedFeaturesService _advancedFeaturesService;

    public GetOrderTimelineQueryHandler(IAdvancedFeaturesService advancedFeaturesService)
    {
        _advancedFeaturesService = advancedFeaturesService;
    }

    public Task<List<TimelineEventDto>> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken)
    {
        return _advancedFeaturesService.GetOrderTimelineAsync(request.OrderId, cancellationToken);
    }
}

// 3. Restore soft-deleted entity Command
public sealed record RestoreEntityCommand(string EntityName, Guid Id) : IRequest<bool>;

public sealed class RestoreEntityCommandHandler : IRequestHandler<RestoreEntityCommand, bool>
{
    private readonly IAdvancedFeaturesService _advancedFeaturesService;

    public RestoreEntityCommandHandler(IAdvancedFeaturesService advancedFeaturesService)
    {
        _advancedFeaturesService = advancedFeaturesService;
    }

    public Task<bool> Handle(RestoreEntityCommand request, CancellationToken cancellationToken)
    {
        return _advancedFeaturesService.RestoreRecordAsync(request.EntityName, request.Id, cancellationToken);
    }
}

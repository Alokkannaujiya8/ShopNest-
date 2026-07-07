using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

public sealed record GlobalSearchQuery(string QueryText, int Limit = 10) : IRequest<GlobalSearchResultDto>;

public sealed class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, GlobalSearchResultDto>
{
    private readonly IAdvancedFeaturesService _advancedFeaturesService;

    public GlobalSearchQueryHandler(IAdvancedFeaturesService advancedFeaturesService)
    {
        _advancedFeaturesService = advancedFeaturesService;
    }

    public Task<GlobalSearchResultDto> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        return _advancedFeaturesService.GlobalSearchAsync(request.QueryText, request.Limit, cancellationToken);
    }
}

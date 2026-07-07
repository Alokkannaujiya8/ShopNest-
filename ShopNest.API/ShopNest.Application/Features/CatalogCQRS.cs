using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;

namespace ShopNest.Application.Features;

public sealed record GetSearchSuggestionsQuery(string Query) : IRequest<IReadOnlyList<ProductDto>>;
public sealed record GetRelatedProductsQuery(Guid ProductId, int Count = 6) : IRequest<IReadOnlyList<ProductDto>>;
public sealed record GetUserSearchHistoryQuery(Guid? UserId) : IRequest<IReadOnlyList<string>>;
public sealed record GetPopularSearchesQuery(int Count = 8) : IRequest<IReadOnlyList<string>>;

public sealed class CatalogCQRSHandlers(IProductService productService) :
    IRequestHandler<GetSearchSuggestionsQuery, IReadOnlyList<ProductDto>>,
    IRequestHandler<GetRelatedProductsQuery, IReadOnlyList<ProductDto>>,
    IRequestHandler<GetUserSearchHistoryQuery, IReadOnlyList<string>>,
    IRequestHandler<GetPopularSearchesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<ProductDto>> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken) =>
        productService.GetSuggestionsAsync(request.Query, cancellationToken);

    public Task<IReadOnlyList<ProductDto>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken) =>
        productService.GetRelatedAsync(request.ProductId, request.Count, cancellationToken);

    public Task<IReadOnlyList<string>> Handle(GetUserSearchHistoryQuery request, CancellationToken cancellationToken) =>
        productService.GetUserSearchHistoryAsync(request.UserId, cancellationToken);

    public Task<IReadOnlyList<string>> Handle(GetPopularSearchesQuery request, CancellationToken cancellationToken) =>
        productService.GetPopularSearchesAsync(request.Count, cancellationToken);
}

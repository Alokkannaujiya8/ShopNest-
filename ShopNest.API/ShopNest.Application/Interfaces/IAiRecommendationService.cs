using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Interfaces;

public interface IAiRecommendationService
{
    Task<List<ProductDto>> GetRecommendationsForUserAsync(Guid userId, int count = 10);
    Task<List<ProductDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int count = 5);
    Task<List<ProductDto>> GetSimilarProductsAsync(Guid productId, int count = 5);
    Task<List<ProductDto>> GetTrendingProductsAsync(int count = 10);
    Task<List<ProductDto>> GetPopularProductsAsync(int count = 10);
}

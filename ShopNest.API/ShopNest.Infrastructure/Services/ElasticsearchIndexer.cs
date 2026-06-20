using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Services;

public sealed class ElasticsearchIndexer(IServiceProvider serviceProvider, ILogger<ElasticsearchIndexer> logger) : ISearchIndexer
{
    private const string IndexName = "shopnest-products";

    public async Task IndexProductAsync(Product product, CancellationToken cancellationToken)
    {
        try
        {
            var client = serviceProvider.GetService<ElasticsearchClient>();
            if (client is null) return;

            var response = await client.IndexAsync(product, x => x.Index(IndexName).Id(product.Id), cancellationToken);
            if (!response.IsValidResponse)
            {
                logger.LogWarning("Elasticsearch product indexing failed for {ProductId}", product.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to connect to Elasticsearch for indexing product {ProductId}", product.Id);
        }
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            var client = serviceProvider.GetService<ElasticsearchClient>();
            if (client is null) return;
            await client.DeleteAsync<Product>(productId, x => x.Index(IndexName), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to connect to Elasticsearch for deleting product {ProductId}", productId);
        }
    }
}

using ShopNest.Application.Dtos;
using ShopNest.Domain.Entities;

namespace ShopNest.Application.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken);
    Task PublishOrderStatusChangedAsync(Order order, CancellationToken cancellationToken);
}

public interface IOrderNotificationService
{
    Task SendOrderConfirmationAsync(Guid orderId);
}

public interface ISearchIndexer
{
    Task IndexProductAsync(Product product, CancellationToken cancellationToken);
    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken);
}

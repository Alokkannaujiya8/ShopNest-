using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Infrastructure.Settings;

namespace ShopNest.Infrastructure.Services;

public sealed class OrderEventPublisher(IOptions<RabbitMqSettings> options, ILogger<OrderEventPublisher> logger) : IOrderEventPublisher
{
    public Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken) => PublishAsync("order.created", order, cancellationToken);
    public Task PublishOrderStatusChangedAsync(Order order, CancellationToken cancellationToken) => PublishAsync("order.status.changed", order, cancellationToken);

    private Task PublishAsync(string routingKey, Order order, CancellationToken cancellationToken)
    {
        try
        {
            var settings = options.Value;
            var factory = new ConnectionFactory { HostName = settings.HostName, UserName = settings.UserName, Password = settings.Password };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(settings.Exchange, ExchangeType.Topic, durable: true);
            var payload = JsonSerializer.Serialize(new { order.Id, order.OrderNumber, order.UserId, order.Status, order.TotalAmount });
            var body = Encoding.UTF8.GetBytes(payload);
            channel.BasicPublish(settings.Exchange, routingKey, basicProperties: null, body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ publish failed for {RoutingKey}", routingKey);
        }

        return Task.CompletedTask;
    }
}

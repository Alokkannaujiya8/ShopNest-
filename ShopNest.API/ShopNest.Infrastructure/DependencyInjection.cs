using Elastic.Clients.Elasticsearch;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ShopNest.Application.Interfaces;
using ShopNest.Infrastructure.Persistence;
using ShopNest.Infrastructure.Services;
using ShopNest.Infrastructure.Settings;

namespace ShopNest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.Configure<PaymentSettings>(configuration.GetSection("Payments"));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing.");

        services.AddDbContext<ShopNestDbContext>(options => options.UseSqlServer(connectionString));

        services.AddHangfire(config => config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true
        }));
        services.AddHangfireServer();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
        }

        var elasticUrl = configuration["Elasticsearch:Url"];
        if (!string.IsNullOrWhiteSpace(elasticUrl))
        {
            services.AddSingleton(new ElasticsearchClient(new Uri(elasticUrl)));
        }

        services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartOrderService, CartOrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
        services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();
        services.AddScoped<IOrderNotificationService, OrderNotificationService>();
        services.AddScoped<ISearchIndexer, ElasticsearchIndexer>();

        return services;
    }
}

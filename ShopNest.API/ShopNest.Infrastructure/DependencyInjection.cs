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
using ShopNest.Infrastructure.Services.Notifications;
using ShopNest.Infrastructure.Settings;
using ShopNest.Infrastructure.Payments;

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

        services.AddHttpContextAccessor();
        services.AddDbContext<ShopNestDbContext>(options => 
            options.UseSqlServer(connectionString, sqlOptions => 
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

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

        services.AddHttpClient();

        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IPaymentProvider, RazorpayPaymentProvider>();
        services.AddScoped<IPaymentProvider, PayPalPaymentProvider>();
        services.AddScoped<IPaymentProvider, CodPaymentProvider>();

        services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICartOrderService, CartOrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IShippingService, ShippingService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        // Multi-channel Notification Providers
        services.AddScoped<INotificationProvider, EmailNotificationProvider>();
        services.AddScoped<INotificationProvider, SMSNotificationProvider>();
        services.AddScoped<INotificationProvider, PushNotificationProvider>();
        services.AddScoped<INotificationProvider, WhatsAppNotificationProvider>();
        services.AddScoped<INotificationProvider, TelegramNotificationProvider>();

        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IAdminManagementService, AdminManagementService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
        services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();
        services.AddScoped<IOrderNotificationService, OrderNotificationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditSearchService, AuditSearchService>();
        services.AddScoped<IAiRecommendationService, AiRecommendationService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IAdvancedFeaturesService, AdvancedFeaturesService>();
        services.AddScoped<ISearchIndexer, ElasticsearchIndexer>();
        services.AddScoped<IReportingService, ReportingService>();

        return services;
    }
}

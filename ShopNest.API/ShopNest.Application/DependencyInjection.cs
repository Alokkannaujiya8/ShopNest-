using Microsoft.Extensions.DependencyInjection;

namespace ShopNest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(cfg => cfg.AddProfile<ShopNest.Application.Common.MappingProfile>());

        return services;
    }
}

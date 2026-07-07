using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ShopNest.Application.Common;

namespace ShopNest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        services.AddAutoMapper(cfg => cfg.AddProfile<ShopNest.Application.Common.MappingProfile>());
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}

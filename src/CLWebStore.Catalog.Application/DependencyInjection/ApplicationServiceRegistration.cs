using CLWebStore.Catalog.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CLWebStore.Catalog.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        // Register FluentValidation
        services.AddValidatorsFromAssembly(executingAssembly);

        // Register MediatR & Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(executingAssembly);

            // Order is important: Trace -> Log -> Validate -> Handle
            cfg.AddOpenBehavior(typeof(TracingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register TimeProvider as a singleton
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

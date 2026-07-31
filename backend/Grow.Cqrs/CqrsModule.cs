using Microsoft.Extensions.DependencyInjection;

namespace Grow.Cqrs;

public static class CqrsModule
{
    public static void RegisterCqrs(this IServiceCollection services)
    {
        _ = services.AddSingleton<IDispatcher, Dispatcher>();

        _ = services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }
}

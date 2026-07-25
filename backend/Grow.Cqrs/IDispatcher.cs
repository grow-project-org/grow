using Microsoft.Extensions.DependencyInjection;

namespace Grow.Cqrs;

public interface IDispatcher
{
    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct) where TQuery : IQuery<TResult>;
    Task SendAsync<TCommand>(TCommand command, CancellationToken ct) where TCommand : ICommand;
}

public interface ICommand;
public interface IEvent;

public static class CqrsModule
{
    public static void RegisterCqrs(this IServiceCollection services)
    {
        services.AddSingleton<IDispatcher, Dispatcher>();

        services.Scan(scan => scan
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


public sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct)
        where TQuery : IQuery<TResult>
    {
        ct.ThrowIfCancellationRequested();
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        return handler.HandleAsync(query, ct);
    }

    public Task SendAsync<TCommand>(TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        ct.ThrowIfCancellationRequested();
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        return handler.HandleAsync(command, ct);
    }
}

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken ct);
}

public interface IQuery<TResult>;

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}

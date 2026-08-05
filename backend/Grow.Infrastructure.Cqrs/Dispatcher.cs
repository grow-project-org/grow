using Grow.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grow.Infrastructure.Cqrs;

public sealed class Dispatcher(IServiceProvider serviceProvider, ILogger<Dispatcher> logger) : IDispatcher
{
    public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct)
        where TQuery : IQuery<TResult>
    {
        ct.ThrowIfCancellationRequested();

        var name = typeof(TQuery).Name;

        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        logger.ExecutingQueryStarted(name);
        var result = await handler.HandleAsync(query, ct);
        logger.ExecutingQueryFinished(name);

        return result;
    }

    public async Task SendAsync<TCommand>(TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        ct.ThrowIfCancellationRequested();

        var name = typeof(TCommand).Name;

        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        logger.ExecutingCommandStarted(name);
        await handler.HandleAsync(command, ct);
        logger.ExecutingCommandFinished(name);
    }
}

namespace Grow.Cqrs;

public interface IDispatcher
{
    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct) where TQuery : IQuery<TResult>;
    Task SendAsync<TCommand>(TCommand command, CancellationToken ct) where TCommand : ICommand;
}

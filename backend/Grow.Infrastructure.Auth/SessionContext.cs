namespace Grow.Infrastructure.Auth;

public record SessionContext(Guid UserId);

public interface ISessionContextProvider
{
    Task<SessionContext?> Get();
}

public class SessionContextProvider : ISessionContextProvider
{
    private SessionContext? _context;

    public Task<SessionContext?> Get() => Task.FromResult(this._context);

    internal void Set(SessionContext context) => this._context = context;
}
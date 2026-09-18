using Microsoft.AspNetCore.Http;

namespace Grow.Infrastructure.Auth;

public class UserSessionProvider(SessionStorage sessionStorage)
{
    private bool initialized;
    public string? SessionKey { get; private set; }
    public HttpContext? Context { get; private set; }

    public void Initialize(string sessionKey, HttpContext httpContext)
    {
        if (this.initialized)
        {
            throw new Exception("UserSessionProvider is already initialized");
        }

        this.SessionKey = sessionKey;
        this.Context = httpContext;
        this.initialized = true;
    }

    public UserSession GetSession()
    {
        if (!this.initialized)
        {
            throw new Exception("UserSessionProvider is not initialized");
        }

        var session = sessionStorage.GetSession(this.SessionKey!, this.Context!);
        return session;
    }

    public void Logout()
    {
        if (this.SessionKey == null)
        {
            throw new ArgumentException("Session key is not set");
        }

        sessionStorage.Logout(this.SessionKey);
    }
}

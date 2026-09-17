using Microsoft.AspNetCore.Http;

namespace Grow.Infrastructure.Auth;

public class UserSessionProvider(SessionStorage sessionStorage)
{
    public string? SessionKey { get; set; }

    public UserSession GetSession(HttpContext context)
    {
        if (this.SessionKey == null)
        {
            throw new ArgumentException("Session key is not set");
        }

        var session = sessionStorage.GetSession(this.SessionKey, context);
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

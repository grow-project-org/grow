using Grow.Domain.Commons;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Grow.Infrastructure.Auth;

public class SessionStorage
{
    //todo store sessions in cache database like KeyDB/Redis
    private readonly ConcurrentDictionary<string, UserSession> activeSessions = new();
    //todo scan for long waiting session and delete them
    private readonly ConcurrentDictionary<string, UserSession> notConfirmedSessions = new();

    public string CreateSession(Guid userId, bool isUserVerified, HttpContext context)
    {
        if (!isUserVerified)
        {
            throw new ArgumentException("User is not verified");
        }

        var confirmationKey = SessionKeyGenerator.Generate();
        var session = new UserSession()
        {
            SessionKey = SessionKeyGenerator.Generate(),
            LoggedOut = false,
            UserId = userId,
            IsUserVerified = isUserVerified,
            UserAgent = context.Request.Headers.UserAgent!,
            SessionCreatedAt = DateTime.UtcNow,
        };

        //verify if session is valid for context that created it
        return session.AllowUserToEnterApp(context, isConfirmed: false) == false
            ? throw new Exception($"Session creation is broken.")
            : this.notConfirmedSessions.TryAdd(confirmationKey, session) == false
            ? throw new Exception($"Cannot save session with key {confirmationKey} for user {userId} in memory")
            : confirmationKey;
    }

    public string ConfirmSession(string confirmationKey, HttpContext context)
    {
        if (this.notConfirmedSessions.Remove(confirmationKey, out var session))
        {
            if (session.AllowUserToEnterApp(context, isConfirmed: false) == false)
            {
                throw new Exception($"Cannot get session for current context");
            }

            var sessionKey = session.SessionKey;
            return this.activeSessions.TryAdd(sessionKey, session) == false
                ? throw new Exception($"Cannot save session with key {sessionKey} for user {session.UserId} in memory")
                : sessionKey;
        }
        else
        {
            throw new Exception($"Cannot get session for current context");
        }
    }

    public UserSession GetSession(string key, HttpContext context)
    {
        if (this.activeSessions.TryGetValue(key, out var session))
        {
            if (session.AllowUserToEnterApp(context, isConfirmed: true) == false)
            {
                _ = this.activeSessions.Remove(key, out _);
                throw new Exception($"Cannot get session for current context");
            }

            return session;
        }

        throw new ArgumentException($"Cannot get session for key {key}");
    }

    public void Logout(string key)
    {
        if (this.activeSessions.ContainsKey(key) == false)
        {
            return;
        }

        if (this.activeSessions.Remove(key, out var session))
        {
            //change flag in memory
            //block flows executed at now
            session.LoggedOut = true;
        }
        else if (this.activeSessions.TryGetValue(key, out var sessionGetValue))
        {
            sessionGetValue.LoggedOut = true;
        }
        else
        {
            throw new ArgumentException($"Cannot logout session {key}");
        }
    }
}

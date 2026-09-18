using Grow.Domain.Commons;
using Microsoft.AspNetCore.Http;

namespace Grow.Infrastructure.Auth;

public class UserSession
{
    private static readonly DateTime MIN_SESSION_CREATED_DATE_TIME = new(2026, 4, 13);

    public required string SessionKey { get; set; }
    public required bool LoggedOut { get; set; }
    public required Guid UserId { get; set; }
    public required bool IsUserVerified { get; set; }
    public required string UserAgent { get; set; }
    public required DateTime SessionCreatedAt { get; set; }
    public DateTime LastApiCall { get; set; }

    public AuthUser ToAuthUser() => new (this.UserId, this.IsUserVerified);

    public bool AllowUserToEnterApp(HttpContext context, bool isConfirmed)
    {
        if (this.LoggedOut)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.SessionKey))
        {
            return false;
        }

        if (this.UserId == Guid.Empty)
        {
            return false;
        }

        if (this.IsUserVerified == false)
        {
            return false;
        }

        if (this.SessionCreatedAt < MIN_SESSION_CREATED_DATE_TIME)
        {
            return false;
        }

        return !this.IsExpired(isConfirmed) && this.VerifyHttpContext(context);
    }

    public bool IsExpired(bool isConfirmed)
    {
        var now = DateTime.UtcNow;
        if (isConfirmed)
        {
            // api call is set
            if (this.LastApiCall > MIN_SESSION_CREATED_DATE_TIME)
            {
                return this.LastApiCall < now.AddDays(-7);
            }
            else
            {
                // user has 15 minutes for first api call after /ConfirmLogin (after ConfirmationKey step)
                return this.SessionCreatedAt < now.AddMinutes(-15);
            }
        }

        // user has 15 minutes for first api call after /Login (ConfirmationKey step)
        return this.SessionCreatedAt < now.AddMinutes(-15);
    }

    private bool VerifyHttpContext(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent;

        return this.UserAgent == userAgent;
    }
}

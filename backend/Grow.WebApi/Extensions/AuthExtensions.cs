using Grow.Infrastructure.Auth;
using System.Security.Claims;

namespace Grow.WebApi.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection SetupCors(this IServiceCollection services)
    {
        _ = services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(["http://localhost:5173"])
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type", "X-CSRF-TOKEN")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

        return services;
    }

    public static void ConfigureSetSession(this WebApplication app)
    {
        _ = app.Use((ctx, next) =>
        {
            if (ctx.User.Identity?.IsAuthenticated == true)
            {
                var sessionKey = ctx.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                var sessionProvider = ctx.RequestServices.GetService<UserSessionProvider>();
                if (sessionProvider != null)
                {
                    sessionProvider.SessionKey = sessionKey;
                    var session = sessionProvider.GetSession(ctx);
                    session.LastApiCall = DateTime.UtcNow;
                }
            }

            return next(ctx);
        });
    }
}

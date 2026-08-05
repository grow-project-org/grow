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
}

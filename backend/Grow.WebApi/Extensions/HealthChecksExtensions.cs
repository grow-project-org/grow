using Grow.Infrastructure.Database;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Grow.WebApi.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection SetupHealthChecks(this IServiceCollection services)
    {
        _ = services.AddHealthChecks().AddDbContextCheck<DatabaseContext>();

        return services;
    }

    //todo setup cors to hide healthcheck
    //it should be available for local network only
    public static WebApplication AddHealthChecks(this WebApplication app)
    {
        _ = app.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json; charset=utf-8";

                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds,
                        description = e.Value.Description,
                        error = e.Value.Exception?.Message,
                    })
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    }));
            }
        });

        return app;
    }
}
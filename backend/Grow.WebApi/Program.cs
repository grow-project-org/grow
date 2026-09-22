using Grow.Infrastructure.Cqrs;
using Grow.Infrastructure.Database;
using Grow.Infrastructure.Logging;
using Grow.WebApi.Endpoints;
using Grow.WebApi.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services
    .SetupCors()
    .AddOpenApi()
    .RegisterCqrs()
    .RegisterLogging()
    .AddValidation()
    .AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromMinutes(5))
    .AddGrowDatabase(o => o.UseNpgsql(postgresConnectionString))
    .SetupHealthChecks();

builder.Services.ConfigureCors(builder.Configuration);
builder.Services.ConfigureAuth();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy(AuthExtensions.MainRateLimiter, httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 1000000,//todo for tests
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 3,
                QueueLimit = 0
            }));

    options.AddPolicy(AuthExtensions.AuthRateLimiter, httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30000,//todo for tests
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 3,
                QueueLimit = 0
            }));
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

app.UseCors();
app.UseRateLimiter();
app.AddHealthChecks();
app.UseHttpsRedirection();

app.UseAuthentication();
app.ConfigureSetSession();

app.UseAuthorization();
app.UseAntiforgery();

app
    .MapPlantsEndpoints()
    .MapSpeciesEndpoints()
    .MapUsersEndpoints();

app.Run();

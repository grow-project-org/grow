using Grow.Infrastructure.Cqrs;
using Grow.Infrastructure.Database;
using Grow.Infrastructure.Logging;
using Grow.WebApi.Endpoints;
using Grow.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

app.UseCors();
app.AddHealthChecks();
app.UseHttpsRedirection();

app
    .MapPlantsEndpoints()
    .MapSpeciesEndpoints();

app.Run();

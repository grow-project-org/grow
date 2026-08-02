using Grow.Cqrs;
using Grow.Infrastructure.Database;
using Grow.Logging;
using Grow.WebApi.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(["http://localhost:5173"])
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type", "X-CSRF-TOKEN")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

builder.Services.AddOpenApi();
builder.Services.RegisterCqrs();
builder.Services.RegisterLogging();

builder.Services.AddValidation();

builder.Services.AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromMinutes(5));

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");
builder.Services.AddGrowDatabase(o => o.UseNpgsql(postgresConnectionString));

builder.Services.AddHealthChecks().AddDbContextCheck<DatabaseContext>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
{
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    opts.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

app.UseCors();

//todo setup cors to hide healthcheck
//it should be available for local network only
app.MapHealthChecks("/hc", new HealthCheckOptions()
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

app.UseHttpsRedirection();

app.MapPlantsEndpoints();

app.Run();

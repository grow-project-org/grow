using Grow.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Grow.Tests.Integration;

public sealed class GrowApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.UseEnvironment("Testing");
        _ = builder.UseSetting("ConnectionStrings:Postgres", "Host=unused");

        _ = builder.ConfigureTestServices(services =>
        {
            _ = services.RemoveGrowDatabase();
            _ = services.AddGrowDatabase(o =>
            {
                _ = o.UseInMemoryDatabase($"GrowTestDB-{Guid.NewGuid()}");
                _ = o.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
        });
    }

    public async Task<DatabaseContext> CreateDbContextAsync()
    {
        var factory = this.Services.GetRequiredService<IDbContextFactory<DatabaseContext>>();
        return await factory.CreateDbContextAsync();
    }
}
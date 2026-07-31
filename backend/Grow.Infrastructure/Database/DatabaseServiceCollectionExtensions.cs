using Grow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Grow.Infrastructure.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddGrowDatabase(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
    {
        _ = services.AddDbContextFactory<DatabaseContext>(configure);
        _ = services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext());
        _ = services.AddScoped<IDatabaseContext>(sp =>
            sp.GetRequiredService<DatabaseContext>());
        return services;
    }
}

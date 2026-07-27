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
        services.AddDbContextFactory<DatabaseContext>(configure);
        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext());
        services.AddScoped<IDatabaseContext>(sp =>
            sp.GetRequiredService<DatabaseContext>());
        return services;
    }
}

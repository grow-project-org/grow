using Grow.Domain;
using Grow.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Grow.Tests.Integration;

public static class ServiceCollectionTestExtensions
{
    public static IServiceCollection RemoveGrowDatabase(this IServiceCollection services)
    {
        _ = services.RemoveAll<DbContextOptions>();
        _ = services.RemoveAll<DbContextOptions<DatabaseContext>>();
        _ = services.RemoveAll(typeof(IDbContextOptionsConfiguration<DatabaseContext>));
        _ = services.RemoveAll<IDbContextFactory<DatabaseContext>>();
        _ = services.RemoveAll<DatabaseContext>();
        _ = services.RemoveAll<IDatabaseContext>();
        return services;
    }
}

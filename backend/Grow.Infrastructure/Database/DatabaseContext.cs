using Grow.Domain;
using Grow.Domain.Plants;
using Microsoft.EntityFrameworkCore;

namespace Grow.Infrastructure.Database;

public class DatabaseContext : DbContext, IDatabaseContext
{
    public DbSet<Plant> Plants { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql($"Host=localhost;Username=postgres;Database=postgres");

    protected override void OnModelCreating(ModelBuilder builder)
    {

    }
}

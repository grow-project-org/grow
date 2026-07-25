using Grow.Domain;
using Grow.Domain.Plants;
using Microsoft.EntityFrameworkCore;

namespace Grow.Infrastructure.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IDatabaseContext
{
    public DbSet<Plant> Plants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Plant>(entity =>
        {
            entity.ToTable("Plants");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.CustomId).IsRequired().HasMaxLength(100);
            entity.Property(p => p.SpecieId).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
        });
    }
}

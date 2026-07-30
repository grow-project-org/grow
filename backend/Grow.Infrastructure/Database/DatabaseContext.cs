using Grow.Domain;
using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Species;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Grow.Infrastructure.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IDatabaseContext
{
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Specie> Species { get; set; }

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

            entity.HasMany(p => p.PlantActionLogs)
                .WithOne()
                .HasForeignKey("PlantId")
                .IsRequired();

            entity.Navigation(p => p.PlantActionLogs)
                .HasField("plantActionLogs")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<PlantActionLog>(entity =>
        {
            entity.ToTable("PlantActionLogs");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.Type).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.ExecutedAt).IsRequired();
        });

        builder.Entity<Specie>(entity =>
        {
            entity.ToTable("Species");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Intervals).IsRequired().HasConversion(x => JsonConvert.SerializeObject(x), x => JsonConvert.DeserializeObject<Dictionary<PlantActionType, TimeSpan>>(x)!);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
        });
    }
}
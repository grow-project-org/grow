using Grow.Domain;
using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Species;
using Grow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Grow.Infrastructure.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IDatabaseContext
{
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Specie> Species { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PlantGroup> PlantGroups { get; set; }

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

        builder.Entity<Specie>(entity => 
        {
            entity.ToTable("Species");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Intervals).IsRequired().HasConversion(x => JsonConvert.SerializeObject(x), x => JsonConvert.DeserializeObject<Dictionary<PlantActionType, TimeSpan>>(x)!);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
        });

        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Username).IsRequired();
        });

        builder.Entity<PlantGroup>(entity =>
        {
            entity.ToTable("PlantGroups");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Type).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
            entity.Metadata
                .FindNavigation(nameof(PlantGroup.Plants))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(pg => pg.Plants)
                .WithMany()
                .UsingEntity(j => j.ToTable("PlantGroupPlants"));
        });
    }
}
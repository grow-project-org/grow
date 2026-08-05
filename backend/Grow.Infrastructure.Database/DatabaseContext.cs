using Grow.Domain;
using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Species;
using Grow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Grow.Infrastructure.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IDatabaseContext
{
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Specie> Species { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PlantGroup> PlantGroups { get; set; }
    public DbSet<PlantEvent> PlantEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        _ = builder.Entity<Plant>(entity =>
        {
            _ = entity.ToTable("Plants");
            _ = entity.HasKey(p => p.Id);
            _ = entity.Property(p => p.Id).ValueGeneratedNever();
            _ = entity.Property(p => p.CustomId).IsRequired().HasMaxLength(100);
            _ = entity.Property(p => p.SpecieId).IsRequired();
            _ = entity.Property(p => p.CreatedAt).IsRequired();
            _ = entity.Property(p => p.UpdatedAt).IsRequired();

            _ = entity.HasOne<Specie>()
                .WithMany()
                .HasForeignKey(p => p.SpecieId)
                .IsRequired();

            _ = entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .IsRequired();

            _ = entity.HasMany(p => p.Events)
                .WithOne()
                .HasForeignKey(x => x.PlantId)
                .IsRequired();

            _ = entity.Navigation(p => p.Events)
                .HasField("events")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            _ = entity.HasMany(p => p.PlantGroupMemberships)
                .WithOne()
                .HasForeignKey(m => m.PlantId)
                .IsRequired();

            _ = entity.Navigation(p => p.PlantGroupMemberships)
                .HasField("plantGroupMemberships")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        _ = builder.Entity<PlantGroupMembership>(entity =>
        {
            _ = entity.ToTable("PlantGroupMemberships");
            _ = entity.HasKey(m => new { m.PlantId, m.PlantGroupId });
        });

        _ = builder.Entity<PlantEvent>(entity =>
        {
            _ = entity.ToTable("PlantEvents");
            _ = entity.HasKey(p => p.Id);
            _ = entity.Property(p => p.Id).ValueGeneratedNever();
            _ = entity.Property(p => p.Type).IsRequired();
            _ = entity.Property(p => p.CreatedAt).IsRequired();
            _ = entity.Property(p => p.ExecutedAt).IsRequired();
        });

        _ = builder.Entity<Specie>(entity =>
        {
            _ = entity.ToTable("Species");
            _ = entity.HasKey(p => p.Id);
            _ = entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            var intervalsProperty = entity.Property(p => p.Intervals).IsRequired()
                .HasConversion(
                    x => JsonConvert.SerializeObject(x),
                    x => JsonConvert.DeserializeObject<Dictionary<PlantActionType, TimeSpan>>(x)!);
            intervalsProperty.Metadata.SetValueComparer(new ValueComparer<Dictionary<PlantActionType, TimeSpan>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                d => d.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
                d => new Dictionary<PlantActionType, TimeSpan>(d)));
            _ = entity.Property(p => p.CreatedAt).IsRequired();
            _ = entity.Property(p => p.UpdatedAt).IsRequired();

            _ = entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .IsRequired();
        });

        _ = builder.Entity<User>(entity =>
        {
            _ = entity.ToTable("Users");
            _ = entity.HasKey(u => u.Id);
            _ = entity.Property(u => u.Id).ValueGeneratedNever();
            _ = entity.Property(u => u.Email).IsRequired();
            _ = entity.Property(u => u.Username).IsRequired();
        });

        _ = builder.Entity<PlantGroup>(entity =>
        {
            _ = entity.ToTable("PlantGroups");
            _ = entity.HasKey(p => p.Id);
            _ = entity.Property(p => p.Id).ValueGeneratedNever();
            _ = entity.Property(p => p.Name).IsRequired();
            _ = entity.Property(p => p.Type).IsRequired();
            _ = entity.Property(p => p.CreatedAt).IsRequired();
            _ = entity.Property(p => p.UpdatedAt).IsRequired();

            _ = entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .IsRequired();

            _ = entity.HasMany(pg => pg.PlantGroupMemberships)
                .WithOne()
                .HasForeignKey(m => m.PlantGroupId)
                .IsRequired();

            _ = entity.Navigation(pg => pg.PlantGroupMemberships)
                .HasField("plantGroupMemberships")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
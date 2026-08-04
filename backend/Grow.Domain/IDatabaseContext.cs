using Grow.Domain.Plants;
using Grow.Domain.Species;
using Grow.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain;

public interface IDatabaseContext
{
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Specie> Species { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PlantGroup> PlantGroups { get; set; }
    public DbSet<PlantEvent> PlantEvents { get; set; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

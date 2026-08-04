using Grow.Domain.Plants;
using Grow.Domain.Species;
using Grow.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain;

public interface IDatabaseContext
{
    DbSet<Plant> Plants { get; set; }
    DbSet<Specie> Species { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<PlantGroup> PlantGroups { get; set; }
    DbSet<PlantEvent> PlantEvents { get; set; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

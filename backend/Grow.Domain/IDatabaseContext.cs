using Grow.Domain.Plants;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain;

public interface IDatabaseContext
{
    DbSet<Plant> Plants { get; }
    DbSet<PlantGroup> PlantGroups { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

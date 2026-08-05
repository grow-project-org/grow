using Grow.Domain.Commons.Ownership;
using System.Collections.ObjectModel;

namespace Grow.Domain.Plants;

public class PlantGroup : IOwnable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GroupType Type { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private readonly Collection<PlantGroupMembership> plantGroupMemberships = [];
    public IReadOnlyCollection<PlantGroupMembership> PlantGroupMemberships => this.plantGroupMemberships;

    public Guid OwnerId { get; }

    private PlantGroup(Guid id, string name, GroupType type, Guid ownerId)
    {
        this.Id = id;
        this.Name = name;
        this.Type = type;
        this.OwnerId = ownerId;
    }

    public static PlantGroup CreateWorkGroup(Guid id, string name, Guid ownerId) => new(id, name, GroupType.WorkGroup, ownerId);
    public static PlantGroup CreateRegion(Guid id, string name, Guid ownerId) => new(id, name, GroupType.Region, ownerId);
    public static PlantGroup CreateTemporaryGroup(Guid id, string name, Guid ownerId) => new(id, name, GroupType.TemporaryGroup, ownerId);

    public void AddPlant(Guid plantId)
    {
        var membership = this.plantGroupMemberships.FirstOrDefault(x => x.PlantId == plantId);
        if (membership != null)
        {
            return;
        }

        var newMembership = PlantGroupMembership.Create(plantId, this.Id);
        this.plantGroupMemberships.Add(newMembership);
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePlant(Guid plantId)
    {
        var membership = this.plantGroupMemberships.FirstOrDefault(x => x.PlantId == plantId);
        if (membership == null)
        {
            return;
        }

        _ = this.plantGroupMemberships.Remove(membership);
        this.UpdatedAt = DateTime.UtcNow;
    }
}

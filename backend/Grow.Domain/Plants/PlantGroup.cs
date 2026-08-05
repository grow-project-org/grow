using System.Collections.ObjectModel;

namespace Grow.Domain.Plants;

public class PlantGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GroupType Type { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private readonly Collection<PlantGroupMembership> plantGroupMemberships = [];
    public IReadOnlyCollection<PlantGroupMembership> PlantGroupMemberships => this.plantGroupMemberships;

    private PlantGroup(Guid id, string name, GroupType type)
    {
        this.Id = id;
        this.Name = name;
        this.Type = type;
    }

    public static PlantGroup CreateWorkGroup(Guid id, string name) => new(id, name, GroupType.WorkGroup);
    public static PlantGroup CreateRegion(Guid id, string name) => new(id, name, GroupType.Region);
    public static PlantGroup CreateTemporaryGroup(Guid id, string name) => new(id, name, GroupType.TemporaryGroup);

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

        this.plantGroupMemberships.Remove(membership);
        this.UpdatedAt = DateTime.UtcNow;
    }
}

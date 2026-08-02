using System.Collections.ObjectModel;

namespace Grow.Domain.Plants;

public class PlantGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GroupType Type { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private readonly Collection<Guid> _plantIds = [];
    public IReadOnlyCollection<Guid> PlantIds => this._plantIds;

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
        if (this._plantIds.Contains(plantId))
        {
            return;    
        }

        this._plantIds.Add(plantId);
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePlant(Guid plantId)
    {
        if (!this._plantIds.Contains(plantId))
        {
            return;
        }

        this._plantIds.Remove(plantId);
        this.UpdatedAt = DateTime.UtcNow;
    }
}

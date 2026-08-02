using Grow.Domain.Commons;
using System.Collections.ObjectModel;

namespace Grow.Domain.Plants;

public class Plant
{
    private readonly Collection<PlantEvent> events = [];

    public Guid Id { get; private set; }
    /// <summary>
    /// User-visible identifier
    /// </summary>
    public string CustomId { get; private set; }
    public Guid SpecieId { get; private set; }
    public IReadOnlyCollection<PlantEvent> Events => this.events;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private readonly Collection<PlantGroup> _plantGroups = [];
    public IReadOnlyCollection<PlantGroup> PlantGroups => this._plantGroups;

    private Plant(Guid id, string customId, Guid specieId)
    {
        this.Id = id;
        this.CustomId = customId;
        this.SpecieId = specieId;
    }

    public static Plant Create(Guid id, string customId, Guid specieId) => new(id, customId, specieId);

    public void AddToGroup(PlantGroup pg)
    {
        if (!this._plantGroups.Contains(pg))
        {
            this._plantGroups.Add(pg);
            pg.AddPlant(this);
            this.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveFromGroup(PlantGroup pg)
    {
        if (this._plantGroups.Contains(pg))
        {
            this._plantGroups.Remove(pg);
            pg.RemovePlant(this);
            this.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetCustomId(string customId)
    {
        this.CustomId = customId;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void SetSpecieId(Guid specieId)
    {
        this.SpecieId = specieId;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void AddEvent(Guid id, PlantActionType type, DateTime executedAt)
    {
        var @event = new PlantEvent(this.Id, id, type, executedAt);
        this.events.Add(@event);
        this.UpdatedAt = DateTime.UtcNow;
    }
}

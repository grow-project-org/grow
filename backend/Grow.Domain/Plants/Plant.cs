using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using System.Collections.ObjectModel;

namespace Grow.Domain.Plants;

public class Plant : IOwnable
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

    private readonly Collection<PlantGroupMembership> plantGroupMemberships = [];
    public IReadOnlyCollection<PlantGroupMembership> PlantGroupMemberships => this.plantGroupMemberships;

    public Guid OwnerId { get; private set; }

    private Plant(Guid id, string customId, Guid specieId, Guid ownerId)
    {
        this.Id = id;
        this.CustomId = customId;
        this.SpecieId = specieId;
        this.OwnerId = ownerId;
    }

    public static Plant Create(Guid id, string customId, Guid specieId, Guid ownerId) => new(id, customId, specieId, ownerId);

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

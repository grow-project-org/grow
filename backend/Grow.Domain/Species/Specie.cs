using Grow.Commons.Extensions;
using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;

namespace Grow.Domain.Species;

public class Specie : IOwnable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Dictionary<PlantActionType, TimeSpan> Intervals = [];
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Guid OwnerId { get; }

    private Specie(Guid id, string name, Guid ownerId)
    {
        this.Id = id;
        this.Name = name;
        this.OwnerId = ownerId;
    }

    public static Specie Create(Guid id, string name, Guid ownerId) => new(id, name, ownerId);

    public void SetWateringInterval(TimeSpan time)
    {
        this.Intervals.AddOrUpdate(PlantActionType.Watering, time);
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void SetFertilizingInterval(TimeSpan time)
    {
        this.Intervals.AddOrUpdate(PlantActionType.Fertilizing, time);
        this.UpdatedAt = DateTime.UtcNow;
    }
}

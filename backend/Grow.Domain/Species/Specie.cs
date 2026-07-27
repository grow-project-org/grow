using Grow.Commons.Extensions;
using Grow.Domain.Commons;

namespace Grow.Domain.Species;

public class Specie
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Dictionary<PlantActionType, TimeSpan> Intervals = [];
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Specie(Guid id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    public static Specie Create(Guid id, string name) => new(id, name);

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

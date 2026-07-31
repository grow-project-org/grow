using Grow.Domain.Commons;

namespace Grow.Domain.Plants;

public class PlantEvent(Guid plantId, Guid id, PlantActionType type, DateTime executedAt)
{
    public Guid Id { get; private set; } = id;
    public Guid PlantId { get; private set; } = plantId;
    public PlantActionType Type { get; private set; } = type;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExecutedAt { get; private set; } = executedAt;
}
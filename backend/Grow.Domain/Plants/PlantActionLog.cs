using Grow.Domain.Commons;

namespace Grow.Domain.Plants;

public class PlantActionLog(Guid id, PlantActionType type, DateTime executedAt)
{
    public Guid Id { get; private set; } = id;
    public PlantActionType Type { get; private set; } = type;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExecutedAt { get; private set; } = executedAt;
}
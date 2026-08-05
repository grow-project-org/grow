namespace Grow.Domain.Plants;

public class PlantGroupMembership
{
    public Guid PlantId { get; private set; }
    public Guid PlantGroupId { get; private set; }
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;

    private PlantGroupMembership() { }

    public static PlantGroupMembership Create(Guid plantId, Guid plantGroupId)
        => new() { PlantId = plantId, PlantGroupId = plantGroupId };
}

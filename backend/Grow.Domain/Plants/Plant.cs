namespace Grow.Domain.Plants;

public class Plant
{
    public Guid Id { get; private set; }
    /// <summary>
    /// User-visible identifier
    /// </summary>
    public string CustomId { get; private set; }
    public Guid SpecieId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Guid PlantGroupId { get; private set; }
    public PlantGroup PlantGroup { get; private set; }

    private Plant(Guid id, string customId, Guid specieId)
    {
        this.Id = id;
        this.CustomId = customId;
        this.SpecieId = specieId;
    }

    public static Plant Create(Guid id, string customId, Guid specieId) => new(id, customId, specieId);

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
}

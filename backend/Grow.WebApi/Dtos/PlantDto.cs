using Grow.Domain.Plants;

namespace Grow.WebApi.Dtos;

public record PlantDto(Guid Id, string CustomId, Guid SpecieId)
{
    public static PlantDto From(Plant plant) => new(plant.Id, plant.CustomId, plant.SpecieId);
}

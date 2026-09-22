using Grow.Domain.Plants;

namespace Grow.WebApi.Dtos;

public record PlantGroupDto(Guid Id, string Name, GroupType Type, IReadOnlyCollection<Guid> PlantIds)
{
    public static PlantGroupDto From(PlantGroup plantGroup)
        => new(
            plantGroup.Id,
            plantGroup.Name,
            plantGroup.Type,
            [.. plantGroup.PlantGroupMemberships.Select(x => x.PlantId)]);
}

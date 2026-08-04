using Grow.Domain.Commons;
using Grow.Domain.Species;

namespace Grow.WebApi.Dtos;

public record SpecieDto(Guid Id, string Name, IReadOnlyDictionary<PlantActionType, TimeSpan> Intervals)
{
    public static SpecieDto From(Specie specie) => new(specie.Id, specie.Name, specie.Intervals);
}

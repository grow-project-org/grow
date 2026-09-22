using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Species;

namespace Grow.WebApi.Dtos;

public record PlantDto(
    Guid Id,
    string CustomId,
    Guid SpecieId,
    IReadOnlyCollection<Guid> PlantGroupIds,
    IReadOnlyDictionary<PlantActionType, DateTime> LastExecutions,
    IReadOnlyDictionary<PlantActionType, DateOnly> NextDates)
{
    public static PlantDto From(Plant plant, Specie specie)
    {
        var lastExecutions = plant.Events
            .GroupBy(x => x.Type)
            .ToDictionary(x => x.Key, x => x.Max(e => e.ExecutedAt));

        var nextDates = lastExecutions
            .Where(x => specie.Intervals.ContainsKey(x.Key))
            .ToDictionary(x => x.Key, x => PlantActionSchedule.CalculateNextDate(x.Value, specie.Intervals[x.Key]));

        return new(
            plant.Id,
            plant.CustomId,
            plant.SpecieId,
            [.. plant.PlantGroupMemberships.Select(x => x.PlantGroupId)],
            lastExecutions,
            nextDates);
    }
}

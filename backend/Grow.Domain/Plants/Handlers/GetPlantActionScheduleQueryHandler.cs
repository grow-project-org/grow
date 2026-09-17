using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record GetPlantActionScheduleQuery(Guid PlantId, PlantActionType Type) : IQuery<GetPlantActionScheduleQueryResult>;
public record GetPlantActionScheduleQueryResult(DateOnly? NextDate);

public class GetPlantActionScheduleQueryHandler(IDatabaseContext databaseContext) : IQueryHandler<GetPlantActionScheduleQuery, GetPlantActionScheduleQueryResult>
{
    public async Task<GetPlantActionScheduleQueryResult> HandleAsync(GetPlantActionScheduleQuery query, CancellationToken ct)
    {
        var plant = await databaseContext.Plants.FirstAsync(x => x.Id == query.PlantId, ct);
        var specie = await databaseContext.Species.FirstAsync(x => x.Id == plant.SpecieId, ct);

        var lastEvent = await databaseContext.PlantEvents
            .Where(x => x.PlantId == plant.Id && x.Type == query.Type)
            .OrderByDescending(x => x.ExecutedAt)
            .FirstOrDefaultAsync(ct);

        if (lastEvent is null)
        {
            return new GetPlantActionScheduleQueryResult(null);
        }

        if (!specie.Intervals.TryGetValue(query.Type, out var interval))
        {
            return new GetPlantActionScheduleQueryResult(null);
        }

        var nextDate = PlantActionSchedule.CalculateNextDate(lastEvent.ExecutedAt, interval);

        return new GetPlantActionScheduleQueryResult(nextDate);
    }
}

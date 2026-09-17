using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record GetPlantActionScheduleCommand(Guid PlantId, PlantActionType Type) : IQuery<GetPlantActionScheduleCommandResult>;
public record GetPlantActionScheduleCommandResult(DateOnly? NextDate);

public class GetPlantActionScheduleCommandHandler(IDatabaseContext databaseContext) : IQueryHandler<GetPlantActionScheduleCommand, GetPlantActionScheduleCommandResult>
{
    public async Task<GetPlantActionScheduleCommandResult> HandleAsync(GetPlantActionScheduleCommand query, CancellationToken ct)
    {
        var plant = await databaseContext.Plants.FirstAsync(x => x.Id == query.PlantId, ct);
        var specie = await databaseContext.Species.FirstAsync(x => x.Id == plant.SpecieId, ct);

        var lastEvent = await databaseContext.PlantEvents
            .Where(x => x.PlantId == plant.Id && x.Type == query.Type)
            .OrderByDescending(x => x.ExecutedAt)
            .FirstOrDefaultAsync(ct);

        if (lastEvent is null)
        {
            return new GetPlantActionScheduleCommandResult(null);
        }

        if (!specie.Intervals.TryGetValue(query.Type, out var interval))
        {
            return new GetPlantActionScheduleCommandResult(null);
        }

        var nextDate = PlantActionSchedule.CalculateNextDate(lastEvent.ExecutedAt, interval);

        return new GetPlantActionScheduleCommandResult(nextDate);
    }
}

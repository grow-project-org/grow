using Grow.Cqrs;
using Grow.Domain.Commons;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record AddPlantEventCommand(Guid PlantId, Guid ActionLogId, PlantActionType Type, DateTime ExecutedAt) : ICommand;

public class AddPlantEventCommandHandler(IDatabaseContext context) : ICommandHandler<AddPlantEventCommand>
{
    public async Task HandleAsync(AddPlantEventCommand command, CancellationToken ct)
    {
        var plant = await context.Plants.FirstAsync(x => x.Id == command.PlantId, ct);
        plant.AddEvent(command.ActionLogId, command.Type, command.ExecutedAt);
        await context.SaveChangesAsync(ct);
    }
}

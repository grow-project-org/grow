using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record AddPlantToGroupCommand(Guid PlantGroupId, Guid PlantId) : ICommand;

public class AddPlantToGroupCommandHandler(IDatabaseContext context) : ICommandHandler<AddPlantToGroupCommand>
{
    public async Task HandleAsync(AddPlantToGroupCommand command, CancellationToken ct)
    {
        var plantGroup = await context.PlantGroups
            .FirstAsync(x => x.Id == command.PlantGroupId, ct);
        var plant = await context.Plants
            .FirstAsync(x => x.Id == command.PlantId, ct);
        plantGroup.AddPlant(plant.Id);
        _ = await context.SaveChangesAsync(ct);
    }
}

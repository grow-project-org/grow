using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record RemovePlantFromGroupCommand(Guid PlantGroupId, Guid PlantId) : ICommand;

public class RemovePlantFromGroupCommandHandler(IDatabaseContext context) : ICommandHandler<RemovePlantFromGroupCommand>
{
    public async Task HandleAsync(RemovePlantFromGroupCommand command, CancellationToken ct)
    {
        var plantGroup = await context.PlantGroups
            .FirstAsync(x => x.Id == command.PlantGroupId, ct);
        var plant = await context.Plants
            .FirstAsync(x => x.Id == command.PlantId, ct);
        plantGroup.RemovePlant(plant.Id);
        _ = await context.SaveChangesAsync(ct);
    }
}

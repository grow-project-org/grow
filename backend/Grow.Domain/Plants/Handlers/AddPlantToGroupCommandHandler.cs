using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record AddPlantToGroupCommand(Guid PlantGroupId, Guid PlantId) : ICommand;

public class AddPlantToGroupCommandHandler(IDatabaseContext context, IAuthUserSessionProvider userSessionProvider) : ICommandHandler<AddPlantToGroupCommand>
{
    public async Task HandleAsync(AddPlantToGroupCommand command, CancellationToken ct)
    {
        var user = userSessionProvider.Get();

        var plantGroup = await context.PlantGroups
            .FirstAsync(x => x.Id == command.PlantGroupId, ct);
        plantGroup.ThrowIfNotOwner(user);

        var plant = await context.Plants
            .FirstAsync(x => x.Id == command.PlantId, ct);
        plant.ThrowIfNotOwner(user);

        plantGroup.AddPlant(plant.Id);
        _ = await context.SaveChangesAsync(ct);
    }
}

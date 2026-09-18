using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record AddPlantEventCommand(Guid PlantId, Guid ActionLogId, PlantActionType Type, DateTime ExecutedAt) : ICommand;

public class AddPlantEventCommandHandler(IDatabaseContext context, IAuthUserSessionProvider userSessionProvider) : ICommandHandler<AddPlantEventCommand>
{
    public async Task HandleAsync(AddPlantEventCommand command, CancellationToken ct)
    {
        var user = userSessionProvider.Get();

        var plant = await context.Plants.FirstAsync(x => x.Id == command.PlantId, ct);
        plant.ThrowIfNotOwner(user);

        plant.AddEvent(command.ActionLogId, command.Type, command.ExecutedAt);
        _ = await context.SaveChangesAsync(ct);
    }
}

using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record CreatePlantGroupCommand(Guid id, string Name, GroupType type) : ICommand;

public class CreatePlantGroupCommandHandler(IDatabaseContext context) : ICommandHandler<CreatePlantGroupCommand>
{
    public async Task HandleAsync(CreatePlantGroupCommand command, CancellationToken ct)
    {
        var nameAlreadyExists = await context.PlantGroups.AnyAsync(x => x.Name == command.Name, ct);

        if (nameAlreadyExists)
        {
            throw new ArgumentException(
                $"Plant group with name '{command.Name}' already exists",
                nameof(command.Name));
        }

        var plantGroup = command.type switch
        {
            GroupType.Region =>
                PlantGroup.CreateRegion(
                    command.id,
                    command.Name,
                    Guid.NewGuid()),

            GroupType.TemporaryGroup =>
                PlantGroup.CreateTemporaryGroup(
                    command.id,
                    command.Name,
                    Guid.NewGuid()),

            GroupType.WorkGroup =>
                PlantGroup.CreateWorkGroup(
                    command.id,
                    command.Name,
                    Guid.NewGuid()),

            _ => throw new ArgumentOutOfRangeException(
                nameof(command.type),
                command.type,
                "Unknown group type.")
        };

        _ = context.PlantGroups.Add(plantGroup);
        _ = await context.SaveChangesAsync(ct);
    }
}

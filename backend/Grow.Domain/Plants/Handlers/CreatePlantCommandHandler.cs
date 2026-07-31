using Grow.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record CreatePlantCommand(Guid Id, string CustomId, Guid SpecieId) : ICommand;

public class CreatePlantCommandHandler(IDatabaseContext context) : ICommandHandler<CreatePlantCommand>
{
    public async Task HandleAsync(CreatePlantCommand command, CancellationToken ct)
    {
        var customIdAlreadyExists = await context.Plants
            .AnyAsync(p => p.CustomId == command.CustomId);

        if (customIdAlreadyExists)
        {
            throw new ArgumentException(
                $"Plant with CustomId '{command.CustomId}' already exists.",
                nameof(command.CustomId));
        }

        var plants = Plant.Create(command.Id, command.CustomId, command.SpecieId);

        _ = context.Plants.Add(plants);
        _ = await context.SaveChangesAsync(ct);
    }
}

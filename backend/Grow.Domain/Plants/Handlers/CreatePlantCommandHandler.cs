using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record CreatePlantCommand(Guid Id, string CustomId, Guid SpecieId) : ICommand;

public class CreatePlantCommandHandler(IDatabaseContext context) : ICommandHandler<CreatePlantCommand>
{
    public async Task HandleAsync(CreatePlantCommand command, CancellationToken ct)
    {
        var specieExists = await context.Species.AnyAsync(x => x.Id == command.SpecieId, ct);
        if (!specieExists)
        {
            throw new ArgumentException($"SpecieId '{command.SpecieId}' not exists.", nameof(command.SpecieId));
        }

        var customIdAlreadyExists = await context.Plants.AnyAsync(p => p.CustomId == command.CustomId, ct);
        if (customIdAlreadyExists)
        {
            throw new ArgumentException(
                $"Plant with CustomId '{command.CustomId}' already exists.",
                nameof(command.CustomId));
        }

        var plants = Plant.Create(command.Id, command.CustomId, command.SpecieId, Guid.NewGuid());

        _ = context.Plants.Add(plants);
        _ = await context.SaveChangesAsync(ct);
    }
}

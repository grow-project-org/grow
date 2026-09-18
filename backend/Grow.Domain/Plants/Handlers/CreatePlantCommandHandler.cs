using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record CreatePlantCommand(Guid Id, string CustomId, Guid SpecieId) : ICommand;

public class CreatePlantCommandHandler(IDatabaseContext context, IAuthUserSessionProvider userSessionProvider) : ICommandHandler<CreatePlantCommand>
{
    public async Task HandleAsync(CreatePlantCommand command, CancellationToken ct)
    {
        var user = userSessionProvider.Get();

        var specie = await context.Species.FirstOrDefaultAsync(x => x.Id == command.SpecieId, ct) 
            ?? throw new ArgumentException($"SpecieId '{command.SpecieId}' not exists.", nameof(command.SpecieId));
        specie.ThrowIfNotOwner(user);

        var customIdAlreadyExists = await context.Plants.AnyAsync(p => p.OwnerId == user.Id && p.CustomId == command.CustomId, ct);
        if (customIdAlreadyExists)
        {
            throw new ArgumentException(
                $"Plant with CustomId '{command.CustomId}' already exists.",
                nameof(command.CustomId));
        }

        var plants = Plant.Create(command.Id, command.CustomId, command.SpecieId, user.Id);

        _ = context.Plants.Add(plants);
        _ = await context.SaveChangesAsync(ct);
    }
}

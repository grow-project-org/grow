using Grow.Infrastructure.Cqrs;

namespace Grow.Domain.Species.Handlers;

public record CreateSpecieCommand(Guid Id, string Name) : ICommand;

public class CreateSpecieCommandHandler(IDatabaseContext databaseContext) : ICommandHandler<CreateSpecieCommand>
{
    public async Task HandleAsync(CreateSpecieCommand command, CancellationToken ct)
    {
        var specie = Specie.Create(command.Id, command.Name);

        await databaseContext.Species.AddAsync(specie, ct);
        await databaseContext.SaveChangesAsync(ct);
    }
}

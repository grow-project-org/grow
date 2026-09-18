using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;

namespace Grow.Domain.Species.Handlers;

public record CreateSpecieCommand(Guid Id, string Name) : ICommand;

public class CreateSpecieCommandHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : ICommandHandler<CreateSpecieCommand>
{
    public async Task HandleAsync(CreateSpecieCommand command, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var specie = Specie.Create(command.Id, command.Name, user.Id);

        _ = await databaseContext.Species.AddAsync(specie, ct);
        _ = await databaseContext.SaveChangesAsync(ct);
    }
}

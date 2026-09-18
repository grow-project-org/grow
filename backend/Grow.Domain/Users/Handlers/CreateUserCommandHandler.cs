using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;

namespace Grow.Domain.Users.Handlers;

public record CreateUserCommand(Guid Id, string Email, string Username) : ICommand;

public class CreateUserCommandHandler(IDatabaseContext databaseContext) : ICommandHandler<CreateUserCommand>
{
    public async Task HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var user = User.Create(command.Id, command.Email, command.Username);

        _ = await databaseContext.Users.AddAsync(user, ct);
        _ = await databaseContext.SaveChangesAsync(ct);
    }
}

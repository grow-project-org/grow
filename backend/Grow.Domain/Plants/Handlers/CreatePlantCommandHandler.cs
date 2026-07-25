using Grow.Cqrs;

namespace Grow.Domain.Plants.Handlers;

public record CreatePlantCommand(Guid Id, string CustomId, Guid SpecieId) : ICommand;

public class CreatePlantCommandHandler : ICommandHandler<CreatePlantCommand>
{
    public Task HandleAsync(CreatePlantCommand command, CancellationToken ct)
    {
        var plant = Plant.Create(command.Id, command.CustomId, command.SpecieId);
        //todo validate and save to  DB
        return Task.CompletedTask;
    }
}

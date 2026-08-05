using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Species.Handlers;

public record UpdateSpecieIntervalCommand(Guid SpecieId, PlantActionType PlantActionType, TimeSpan Interval) : ICommand;

public class UpdateSpecieIntervalCommandHandler(IDatabaseContext databaseContext) : ICommandHandler<UpdateSpecieIntervalCommand>
{
    public async Task HandleAsync(UpdateSpecieIntervalCommand command, CancellationToken ct)
    {
        var specie = await databaseContext.Species.FirstOrDefaultAsync(x => x.Id == command.SpecieId, ct) 
            ?? throw new ArgumentException($"SpecieId '{command.SpecieId}' not exists.", nameof(command.SpecieId));

        switch (command.PlantActionType)
        {
            case PlantActionType.Watering:
                specie.SetWateringInterval(command.Interval);
                break;
            case PlantActionType.Fertilizing:
                specie.SetFertilizingInterval(command.Interval);
                break;
            default:
                throw new NotImplementedException();
        }

        await databaseContext.SaveChangesAsync(ct);
    }
}

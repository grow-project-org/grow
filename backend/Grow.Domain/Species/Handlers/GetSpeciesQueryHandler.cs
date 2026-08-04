using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Species.Handlers;

public record GetSpeciesQuery : IQuery<GetSpeciesQueryResult>;
public record GetSpeciesQueryResult(IEnumerable<Specie> Species);

public class GetSpeciesQueryHandler(IDatabaseContext databaseContext) : IQueryHandler<GetSpeciesQuery, GetSpeciesQueryResult>
{
    public async Task<GetSpeciesQueryResult> HandleAsync(GetSpeciesQuery query, CancellationToken ct)
    {
        var species = await databaseContext.Species.ToArrayAsync(ct);
        return new GetSpeciesQueryResult(species);
    }
}

using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Species.Handlers;

public record GetSpeciesQuery : IQuery<GetSpeciesQueryResult>;
public record GetSpeciesQueryResult(IEnumerable<Specie> Species);

public class GetSpeciesQueryHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<GetSpeciesQuery, GetSpeciesQueryResult>
{
    public async Task<GetSpeciesQueryResult> HandleAsync(GetSpeciesQuery query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var species = await databaseContext.Species.Where(x => x.OwnerId == Guid.Empty || x.OwnerId == user.Id).ToArrayAsync(ct);
        return new GetSpeciesQueryResult(species);
    }
}

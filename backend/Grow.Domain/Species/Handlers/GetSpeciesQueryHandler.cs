using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Species.Handlers;

public record GetSpeciesQuery(int Skip, int Take, string? SearchName) : IQuery<GetSpeciesQueryResult>;
public record GetSpeciesQueryResult(IEnumerable<Specie> Species);

public class GetSpeciesQueryHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<GetSpeciesQuery, GetSpeciesQueryResult>
{
    public async Task<GetSpeciesQueryResult> HandleAsync(GetSpeciesQuery query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var species = await databaseContext.Species
            .Where(x => x.OwnerId == Guid.Empty || x.OwnerId == user.Id)
            .Where(x => query.SearchName == null || x.Name.Contains(query.SearchName))
            .Skip(query.Skip)
            .Take(query.Take)
            .ToArrayAsync(ct);
        return new GetSpeciesQueryResult(species);
    }
}

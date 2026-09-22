using Grow.Domain.Commons;
using Grow.Domain.Species;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record SearchPlantsQuery(string? SearchText, int From, int Limit) : IQuery<SearchPlantsQueryResult>;
public record SearchPlantsQueryResult(IEnumerable<Plant> Plants, IReadOnlyDictionary<Guid, Specie> Species);

public class SearchPlantsQueryHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<SearchPlantsQuery, SearchPlantsQueryResult>
{
    public async Task<SearchPlantsQueryResult> HandleAsync(SearchPlantsQuery query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var plantsQuery = databaseContext.Plants
            .Include(x => x.Events)
            .Include(x => x.PlantGroupMemberships)
            .Where(x => x.OwnerId == user.Id);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var specieIds = await databaseContext.Species
                .Where(x => x.Name.Contains(query.SearchText))
                .Select(x => x.Id)
                .ToArrayAsync(ct);

            plantsQuery = plantsQuery.Where(x => query.SearchText == null ||
                x.CustomId.Contains(query.SearchText) ||
                specieIds.Contains(x.SpecieId));
        }

        plantsQuery = plantsQuery.Skip(query.From).Take(query.Limit);

        var plants = await plantsQuery.ToArrayAsync(ct);

        var specieIdsOfPlants = plants.Select(x => x.SpecieId).Distinct().ToArray();
        var species = await databaseContext.Species
            .Where(x => specieIdsOfPlants.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return new SearchPlantsQueryResult(plants, species);
    }
}

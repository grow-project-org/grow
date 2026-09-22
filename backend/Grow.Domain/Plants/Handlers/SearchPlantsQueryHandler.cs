using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record SearchPlantsQuery(string? SearchText, int From, int Limit) : IQuery<SearchPlantsQueryResult>;
public record SearchPlantsQueryResult(IEnumerable<Plant> Plants);

public class SearchPlantsQueryHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<SearchPlantsQuery, SearchPlantsQueryResult>
{
    public async Task<SearchPlantsQueryResult> HandleAsync(SearchPlantsQuery query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var plantsQuery = databaseContext.Plants.Where(x => x.OwnerId == user.Id);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            plantsQuery = plantsQuery.Where(x => 
                x.CustomId.Contains(query.SearchText) ||
                databaseContext.Species.Any(specie => specie.Id == x.SpecieId && specie.Name.Contains(query.SearchText)));
        }

        plantsQuery = plantsQuery.Skip(query.From).Take(query.Limit);

        var plants = await plantsQuery.ToArrayAsync(ct);

        return new SearchPlantsQueryResult(plants);
    }
}

using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record SearchPlantsCommand(string? SearchText) : IQuery<SearchPlantsCommandResult>;
public record SearchPlantsCommandResult(IEnumerable<Plant> Plants);

public class SearchPlantsCommandHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<SearchPlantsCommand, SearchPlantsCommandResult>
{
    public async Task<SearchPlantsCommandResult> HandleAsync(SearchPlantsCommand query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var plantsQuery = databaseContext.Plants.Where(x => x.OwnerId == user.Id);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            plantsQuery = plantsQuery.Where(x => 
                x.CustomId.Contains(query.SearchText) ||
                databaseContext.Species.Any(specie => specie.Id == x.SpecieId && specie.Name.Contains(query.SearchText)));
        }

        var plants = await plantsQuery.ToArrayAsync(ct);

        return new SearchPlantsCommandResult(plants);
    }
}

using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record GetPlantGroupsQuery(int From, int Limit, string? SearchName) : IQuery<GetPlantGroupsQueryResult>;
public record GetPlantGroupsQueryResult(IEnumerable<PlantGroup> PlantGroups);

public class GetPlantGroupsQueryHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<GetPlantGroupsQuery, GetPlantGroupsQueryResult>
{
    public async Task<GetPlantGroupsQueryResult> HandleAsync(GetPlantGroupsQuery query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var plantGroups = await databaseContext.PlantGroups
            .Include(x => x.PlantGroupMemberships)
            .Where(x => x.OwnerId == user.Id)
            .Where(x => query.SearchName == null || x.Name.Contains(query.SearchName))
            .Skip(query.From)
            .Take(query.Limit)
            .ToArrayAsync(ct);

        return new GetPlantGroupsQueryResult(plantGroups);
    }
}

using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Users.Handlers;

public record GetUserByIdQuery(Guid UserId) : IQuery<GetUserByIdQueryResult>;
public record GetUserByIdQueryResult(User? User);

public class GetUserByIdQueryHandler(IDatabaseContext databaseContext) : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResult>
{
    public async Task<GetUserByIdQueryResult> HandleAsync(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await databaseContext.Users.FirstOrDefaultAsync(x => x.Id == query.UserId, ct);
        return new (user);
    }
}

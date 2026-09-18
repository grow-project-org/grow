using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Users.Handlers;

public record GetUserByEmailQuery(string Email) : IQuery<GetUserByEmailQueryResult>;
public record GetUserByEmailQueryResult(User? User);

public class GetUserByEmailQueryHandler(IDatabaseContext databaseContext) : IQueryHandler<GetUserByEmailQuery, GetUserByEmailQueryResult>
{
    public async Task<GetUserByEmailQueryResult> HandleAsync(GetUserByEmailQuery query, CancellationToken ct)
    {
        var user = await databaseContext.Users.FirstOrDefaultAsync(x => x.Email == query.Email, ct);
        return new(user);
    }
}

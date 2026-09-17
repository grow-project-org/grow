using Grow.Domain.Users.Handlers;
using Grow.Infrastructure.Cqrs;
using Microsoft.AspNetCore.Mvc;

namespace Grow.WebApi.Endpoints;

public record CreateUserRequest(string Email, string Username);
public record CreateUserResponse(Guid Id);

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        _ = group.MapPost("/register", Register);

        return app;
    }

    public static async Task<CreateUserResponse> Register(IDispatcher dispatcher, [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new CreateUserCommand(id, request.Email, request.Username), ct);
        return new(id);
    }
}

using Grow.Commons.Throttling;
using Grow.Domain.Users.Handlers;
using Grow.Infrastructure.Auth;
using Grow.Infrastructure.Cqrs;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace Grow.WebApi.Endpoints;

public record CreateUserRequest(string Email, string Username);
public record CreateUserResponse(Guid Id);

public record MeResponse(string Username);

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        _ = group.MapPost("/register", Register);
        _ = group.MapPost("/login", Login);
        _ = group.MapPost("/me", GetMe);

        _ = group.MapGet("/csrf", (IAntiforgery forgery, HttpContext ctx) =>
        {
            var tokens = forgery.GetAndStoreTokens(ctx);
            return tokens.RequestToken;
        }).AllowAnonymous();

        return app;
    }

    public static async Task Register(IDispatcher dispatcher, UniversalThrottle throttle, HttpContext http, [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (!throttle.TryAcquire(nameof(Register), request.Email))
        {
            http.Response.StatusCode = 429;
            return;
        }

        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new CreateUserCommand(id, request.Email, request.Username), ct);
    }

    public static async Task Login(IDispatcher dispatcher, UniversalThrottle throttle, HttpContext http, SessionStorage sessionStorage, [FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!throttle.TryAcquire(nameof(Login), request.Email))
        {
            http.Response.StatusCode = 429;
            return;
        }

        var user = (await dispatcher.QueryAsync<GetUserByEmailQuery, GetUserByEmailQueryResult>(new GetUserByEmailQuery(request.Email), ct)).User;

        //todo add real confirmation
        //this is shortcut for MVP
        var confirmationKey = sessionStorage.CreateSession(new AuthUser(user!.Id, true), http);
        var sessionKey = sessionStorage.ConfirmSession(confirmationKey, http);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sessionKey),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    public static Task Logout(HttpContext http, UserSessionProvider sessionProvider, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        sessionProvider.Logout();
        return http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public static async Task<MeResponse> GetMe(IDispatcher dispatcher, HttpContext http, UserSessionProvider sessionProvider, CancellationToken ct)
    {
        var session = sessionProvider.GetSession(http);
        var user = (await dispatcher.QueryAsync<GetUserByIdQuery, GetUserByIdQueryResult>(new GetUserByIdQuery(session.UserId), ct)).User;

        return new MeResponse(user!.Username);
    }
}

using Grow.Domain.Commons;
using Grow.Domain.Species.Handlers;
using Grow.Infrastructure.Cqrs;
using Grow.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Grow.WebApi.Endpoints;

public record CreateSpecieRequest(string Name);
public record CreateSpecieResponse(Guid SpecieId);
public record UpdateIntervalRequest(TimeSpan Interval);

public static class SpeciesEndpoints
{
    public static IEndpointRouteBuilder MapSpeciesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/species").WithTags("Species");

        _ = group.MapGet("/", GetSpecies);
        _ = group.MapPost("/", CreateSpecie);
        _ = group.MapPost("/{specieId:guid}/interval/{actionType}", UpdateInterval);

        return app;
    }

    public static async Task<IEnumerable<SpecieDto>> GetSpecies(IDispatcher dispatcher, CancellationToken ct)
    {
        var species = (await dispatcher.QueryAsync<GetSpeciesQuery, GetSpeciesQueryResult>(new GetSpeciesQuery(), ct)).Species;
        return species.Select(SpecieDto.From);
    }

    public static async Task<CreateSpecieResponse> CreateSpecie(IDispatcher dispatcher, [FromBody] CreateSpecieRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new CreateSpecieCommand(id, request.Name), ct);
        return new(id);
    }

    public static async Task<IResult> UpdateInterval(IDispatcher dispatcher, [FromRoute] Guid specieId, [FromRoute] string actionType, [FromBody] UpdateIntervalRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PlantActionType>(actionType, out var plantActionType))
        {
            return Results.NotFound();
        }

        await dispatcher.SendAsync(new UpdateSpecieIntervalCommand(specieId, plantActionType, request.Interval), ct);

        return Results.Ok();
    }
}
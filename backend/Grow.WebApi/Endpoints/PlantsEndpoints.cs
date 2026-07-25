using Grow.Cqrs;
using Grow.Domain.Plants.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Grow.WebApi.Endpoints;

public record CreatePlantRequest(string CustomId, Guid SpecieId);
public record CreatePlantResponse(Guid CreatedPlantId);

public static class PlantsEndpoints
{
    public static IEndpointRouteBuilder MapPlantsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plants").WithTags("Plants");

        group.MapPost("/", Post);

        return app;
    }

    public static async Task<CreatePlantResponse> Post(IDispatcher dispatcher, [FromBody] CreatePlantRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new CreatePlantCommand(id, request.CustomId, request.SpecieId), ct);
        return new(id);
    }
}

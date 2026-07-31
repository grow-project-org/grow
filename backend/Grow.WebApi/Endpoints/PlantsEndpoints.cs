using Grow.Cqrs;
using Grow.Domain.Commons;
using Grow.Domain.Plants.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Grow.WebApi.Endpoints;

public record CreatePlantRequest(string CustomId, Guid SpecieId);
public record CreatePlantResponse(Guid CreatedPlantId);

public record AddEventRequest(Guid PlantId, PlantActionType Type, DateTime ExecutedAt);
public record AddEventResponse(Guid PlantEventId);

public static class PlantsEndpoints
{
    public static IEndpointRouteBuilder MapPlantsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plants").WithTags("Plants");

        _ = group.MapPost("/", CreatePlant);
        _ = group.MapPost("/{id:guid}/events", AddEvent);

        return app;
    }

    public static async Task<CreatePlantResponse> CreatePlant(IDispatcher dispatcher, [FromBody] CreatePlantRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new CreatePlantCommand(id, request.CustomId, request.SpecieId), ct);
        return new(id);
    }

    public static async Task<AddEventResponse> AddEvent(IDispatcher dispatcher, [FromBody] AddEventRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new AddPlantEventCommand(request.PlantId, id, request.Type, request.ExecutedAt), ct);
        return new(id);
    }
}

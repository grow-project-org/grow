using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Grow.Infrastructure.Cqrs;
using Microsoft.AspNetCore.Mvc;

namespace Grow.WebApi.Endpoints;

public record CreatePlantRequest(string CustomId, Guid SpecieId);
public record CreatePlantResponse(Guid CreatedPlantId);

public record AddEventRequest(Guid PlantId, PlantActionType Type, DateTime ExecutedAt);
public record AddEventResponse(Guid PlantEventId);

public record AddPlantToGroupRequest(Guid PlantGroupId, Guid PlantId);
public record AddPlantToGroupResponse(Guid PlantGroupId, Guid PlantId);

public record RemovePlantFromGroupRequest(Guid PlantGroupId, Guid PlantId);
public record RemovePlantFromGroupResponse(Guid PlantGroupId, Guid PlantId);

public record CreatePlantGroupRequest(string Name, GroupType Type);
public record CreatePlantGroupResponse(Guid CreatedPlantGroupId);

public static class PlantsEndpoints
{
    public static IEndpointRouteBuilder MapPlantsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plants").WithTags("Plants");

        _ = group.MapPost("/", CreatePlant);
        _ = group.MapPost("/{id:guid}/events", AddEvent);
        _ = group.MapPost("/{plantId:guid}/groups/{plantGroupId:guid}", AddToGroup);
        _ = group.MapDelete("/{plantId:guid}/groups/{plantGroupId:guid}", RemoveFromGroup);

        var plantGroup = app.MapGroup("/api/plant-groups").WithTags("Plant Groups");

        _ = plantGroup.MapPost("/", CreateGroup);

        return app;
    }

    public static async Task<CreatePlantGroupResponse> CreateGroup(IDispatcher dispatcher, [FromBody] CreatePlantGroupRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();

        await dispatcher.SendAsync(
            new CreatePlantGroupCommand(
                id,
                request.Name,
                request.Type),
            ct);

        return new(id);
    }

    public static async Task<RemovePlantFromGroupResponse> RemoveFromGroup(IDispatcher dispatcher, [FromBody] RemovePlantFromGroupRequest request, CancellationToken ct)
    {
        await dispatcher.SendAsync(
            new RemovePlantFromGroupCommand(request.PlantGroupId, request.PlantId), ct);
        return new(
            request.PlantGroupId, request.PlantId);
    }

    public static async Task<AddPlantToGroupResponse> AddToGroup(IDispatcher dispatcher, [FromBody] AddPlantToGroupRequest request, CancellationToken ct)
    {
        await dispatcher.SendAsync(
            new AddPlantToGroupCommand(request.PlantGroupId, request.PlantId), ct);
        return new(
            request.PlantGroupId, request.PlantId);
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

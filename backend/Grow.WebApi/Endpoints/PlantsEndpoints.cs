using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Grow.Infrastructure.Cqrs;
using Grow.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Grow.WebApi.Endpoints;

public record CreatePlantRequest(string CustomId, Guid SpecieId);
public record CreatePlantResponse(Guid CreatedPlantId);

public record AddEventRequest(PlantActionType Type, DateTime ExecutedAt);
public record AddEventResponse(Guid PlantEventId);

public record CreatePlantGroupRequest(string Name, GroupType Type);
public record CreatePlantGroupResponse(Guid CreatedPlantGroupId);

public static class PlantsEndpoints
{
    public static IEndpointRouteBuilder MapPlantsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plants").WithTags("Plants");

        _ = group.MapPost("/", CreatePlant);
        _ = group.MapPost("/{plantId:guid}/events", AddEvent);
        _ = group.MapPost("/{plantId:guid}/groups/{plantGroupId:guid}", AddToGroup);
        _ = group.MapDelete("/{plantId:guid}/groups/{plantGroupId:guid}", RemoveFromGroup);
        _ = group.MapGet("/", SearchPlants);

        var plantGroup = app.MapGroup("/api/plant-groups").WithTags("Plant Groups");

        _ = plantGroup.MapPost("/", CreateGroup);

        return app;
    }

    public static async Task<IEnumerable<PlantDto>> SearchPlants(IDispatcher dispatcher, [FromQuery] string? searchText, CancellationToken ct)
    {
        var plants = await dispatcher.QueryAsync<SearchPlantsQuery, SearchPlantsQueryResult>(new SearchPlantsQuery(searchText), ct);
        return plants.Plants.Select(PlantDto.From);
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

    public static async Task<CreatePlantResponse> CreatePlant(IDispatcher dispatcher, [FromBody] CreatePlantRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new CreatePlantCommand(id, request.CustomId, request.SpecieId), ct);
        return new(id);
    }

    public static async Task<AddEventResponse> AddEvent(IDispatcher dispatcher, [FromRoute] Guid plantId, [FromBody] AddEventRequest request, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await dispatcher.SendAsync(new AddPlantEventCommand(plantId, id, request.Type, request.ExecutedAt), ct);
        return new(id);
    }

    public static async Task AddToGroup(IDispatcher dispatcher, [FromRoute] Guid plantId, [FromRoute] Guid plantGroupId, CancellationToken ct)
        => await dispatcher.SendAsync(new AddPlantToGroupCommand(plantGroupId, plantId), ct);

    public static async Task RemoveFromGroup(IDispatcher dispatcher, [FromRoute] Guid plantId, [FromRoute] Guid plantGroupId, CancellationToken ct)
        => await dispatcher.SendAsync(new RemovePlantFromGroupCommand(plantGroupId, plantId), ct);
}

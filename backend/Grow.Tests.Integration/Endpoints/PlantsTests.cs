using Grow.Domain.Commons;
using Grow.WebApi.Endpoints;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration.Endpoints;

[TestFixture]
public class PlantsTests : IntegrationTestBase
{
    [Test]
    public async Task CreatePlant_ShouldReturnPlantId()
    {
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", Guid.NewGuid());

        var response = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        Assert.That(result!.CreatedPlantId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CreatePlant_ShouldPersistPlantWithGivenCustomIdAndSpecieId()
    {
        var customId = $"monstera-{Guid.NewGuid()}";
        var specieId = Guid.NewGuid();
        var newPlantCommand = new CreatePlantRequest(customId, specieId);

        var response = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();

        await using var db = await this.factory.CreateDbContextAsync();
        var plant = await db.Plants.FindAsync(result!.CreatedPlantId);

        Assert.That(plant, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(plant!.CustomId, Is.EqualTo(customId));
            Assert.That(plant.SpecieId, Is.EqualTo(specieId));
        }
    }

    [Test]
    public void CreatePlant_WithDuplicateCustomId_ShouldThrowArgumentException()
    {
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", Guid.NewGuid());

        _ = Assert.CatchAsync<ArgumentException>(async () =>
        {
            _ = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
            _ = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
        });
    }

    [Test]
    public async Task AddEvent_ShouldReturnEventIdAndPersistEvent()
    {
        var plantId = await this.CreatePlantAsync();
        var executedAt = DateTime.UtcNow.AddHours(-1);
        var addEventRequest = new AddEventRequest(plantId, PlantActionType.Watering, executedAt);

        var response = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", addEventRequest);
        var result = await response.Content.ReadFromJsonAsync<AddEventResponse>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        Assert.That(result!.PlantEventId, Is.Not.EqualTo(Guid.Empty));

        await using var db = await this.factory.CreateDbContextAsync();
        var plant = await db.Plants.Include(p => p.Events).FirstAsync(p => p.Id == plantId);

        Assert.That(plant.Events, Has.Count.EqualTo(1));

        var @event = plant.Events.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(@event.Id, Is.EqualTo(result.PlantEventId));
            Assert.That(@event.Type, Is.EqualTo(PlantActionType.Watering));
            Assert.That(@event.ExecutedAt, Is.EqualTo(executedAt));
        }
    }

    [Test]
    public void AddEvent_WhenPlantDoesNotExist_ShouldThrow()
    {
        var addEventRequest = new AddEventRequest(Guid.NewGuid(), PlantActionType.Watering, DateTime.UtcNow);

        _ = Assert.CatchAsync<InvalidOperationException>(() =>
            this.client.PostAsJsonAsync($"/api/plants/{Guid.NewGuid()}/events", addEventRequest));
    }
}

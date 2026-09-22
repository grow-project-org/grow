using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Domain.Plants;
using Grow.WebApi.Dtos;
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
        var specieId = await this.CreateSpecieAsync();
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", specieId);

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
        var specieId = await this.CreateSpecieAsync();
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
    public async Task CreatePlant_WithDuplicateCustomId_ShouldThrowArgumentException()
    {
        var specieId = await this.CreateSpecieAsync();
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", specieId);

        _ = Assert.CatchAsync<ArgumentException>(async () =>
        {
            _ = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
            _ = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
        });
    }

    [Test]
    public void CreatePlant_WhenSpecieDoesNotExist_ShouldThrowArgumentException()
    {
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", Guid.NewGuid());

        _ = Assert.CatchAsync<ArgumentException>(() =>
            this.client.PostAsJsonAsync("/api/plants", newPlantCommand));
    }

    [Test]
    public async Task CreatePlant_ShouldPersistPlantOwnedByLoggedInUser()
    {
        var plantId = await this.CreatePlantAsync();

        await using var db = await this.factory.CreateDbContextAsync();
        var plant = await db.Plants.FindAsync(plantId);

        Assert.That(plant, Is.Not.Null);
        Assert.That(plant!.OwnerId, Is.EqualTo(this.currentUserId));
    }

    [Test]
    public void CreatePlant_WhenSpecieBelongsToAnotherUser_ShouldThrowOwnershipException()
    {
        _ = Assert.CatchAsync<OwnershipException>(async () =>
        {
            var (otherClient, _) = await this.CreateOtherUserClientAsync();
            var otherUsersSpecieResponse = await otherClient.PostAsJsonAsync("/api/species", new CreateSpecieRequest($"secret-{Guid.NewGuid()}"));
            var otherUsersSpecie = await otherUsersSpecieResponse.Content.ReadFromJsonAsync<CreateSpecieResponse>();

            var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", otherUsersSpecie!.SpecieId);
            _ = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);
        });
    }

    [Test]
    public async Task AddEvent_ShouldReturnEventIdAndPersistEvent()
    {
        var plantId = await this.CreatePlantAsync();
        var executedAt = DateTime.UtcNow.AddHours(-1);
        var addEventRequest = new AddEventRequest(PlantActionType.Watering, executedAt);

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
        var addEventRequest = new AddEventRequest(PlantActionType.Watering, DateTime.UtcNow);

        _ = Assert.CatchAsync<InvalidOperationException>(() =>
            this.client.PostAsJsonAsync($"/api/plants/{Guid.NewGuid()}/events", addEventRequest));
    }

    [Test]
    public void AddEvent_WhenPlantBelongsToAnotherUser_ShouldThrowOwnershipException()
    {
        _ = Assert.CatchAsync<OwnershipException>(async () =>
        {
            var (otherClient, _) = await this.CreateOtherUserClientAsync();
            var otherUsersSpecieResponse = await otherClient.PostAsJsonAsync("/api/species", new CreateSpecieRequest($"secret-{Guid.NewGuid()}"));
            var otherUsersSpecie = await otherUsersSpecieResponse.Content.ReadFromJsonAsync<CreateSpecieResponse>();
            var otherUsersPlantResponse = await otherClient.PostAsJsonAsync("/api/plants", new CreatePlantRequest($"monstera-{Guid.NewGuid()}", otherUsersSpecie!.SpecieId));
            var otherUsersPlant = await otherUsersPlantResponse.Content.ReadFromJsonAsync<CreatePlantResponse>();

            var addEventRequest = new AddEventRequest(PlantActionType.Watering, DateTime.UtcNow);
            _ = await this.client.PostAsJsonAsync($"/api/plants/{otherUsersPlant!.CreatedPlantId}/events", addEventRequest);
        });
    }

    [Test]
    public async Task SearchPlants_ByCustomId_ShouldReturnMatchingPlants()
    {
        var specieId = await this.CreateSpecieAsync();

        var matchingOnePlant = new CreatePlantRequest("monstera", specieId);
        var matchingTwoPlant = new CreatePlantRequest("bambus", specieId);

        _ = await this.client.PostAsJsonAsync("/api/plants", matchingOnePlant);
        _ = await this.client.PostAsJsonAsync("/api/plants", matchingTwoPlant);

        var response = await this.client.GetAsync("/api/plants?searchText=monstera&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using(Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result![0].CustomId, Is.EqualTo("monstera"));
        }
    }

    [Test]
    public async Task SearchPlants_BySpecieName_ShouldReturnMatchingPlants()
    {
        var specieId = await this.CreateSpecieAsync("Bambus");

        var request = new CreatePlantRequest("my-bambus", specieId);
        var createResponse = await this.client.PostAsJsonAsync("/api/plants", request);
        var createdPlant = await createResponse.Content.ReadFromJsonAsync<CreatePlantResponse>();

        var response = await this.client.GetAsync("/api/plants?searchText=Bambus&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result![0].Id, Is.EqualTo(createdPlant!.CreatedPlantId));
        }
    }

    [Test]
    public async Task SearchPlants_WithoutSearchText_ShouldReturnAllUserPlants()
    {
        var specieId = await this.CreateSpecieAsync("Bambus");

        var oneRequest = new CreatePlantRequest("my-bambus", specieId);
        var twoRequest = new CreatePlantRequest("my-bambus-2", specieId);
 
        var createOneResponse = await this.client.PostAsJsonAsync("/api/plants", oneRequest);
        var createTwoResponse = await this.client.PostAsJsonAsync("/api/plants", twoRequest);

        var onePlant = await createOneResponse.Content.ReadFromJsonAsync<CreatePlantResponse>();
        var twoPlant = await createTwoResponse.Content.ReadFromJsonAsync<CreatePlantResponse>();

        var response = await this.client.GetAsync("/api/plants?from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result!.Select(x => x.Id), Does.Contain(onePlant!.CreatedPlantId));
            Assert.That(result!.Select(x => x.Id), Does.Contain(twoPlant!.CreatedPlantId));
        }
    }

    [Test]
    public async Task SearchPlants_ShouldNotReturnPlantsOwnedByAnotherUser()
    {
        var (client, _) = await this.CreateOtherUserClientAsync();

        var specieResponse = await client.PostAsJsonAsync("/api/species", new CreateSpecieRequest("Bambus One"));
        var specie = await specieResponse.Content.ReadFromJsonAsync<CreateSpecieResponse>();

        var plantResponse = await client.PostAsJsonAsync("/api/plants", new CreatePlantRequest("bambus-one", specie!.SpecieId));

        Assert.That(plantResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var response = await this.client.GetAsync("/api/plants?searchText=bambus-one&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }

    [Test]
    public async Task SearchPlants_WhenPlantHasNoEvents_ShouldReturnEmptyLastExecutionsAndNextDates()
    {
        _ = await this.CreatePlantAsync("no-events");

        var response = await this.client.GetAsync("/api/plants?searchText=no-events&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result![0].LastExecutions, Is.Empty);
            Assert.That(result![0].NextDates, Is.Empty);
        }
    }

    [Test]
    public async Task SearchPlants_WhenSpecieHasNoInterval_ShouldReturnLastExecutionWithoutNextDate()
    {
        var specieId = await this.CreateSpecieAsync();
        var plantId = await this.CreatePlantAsync("no-interval", specieId);
        var executedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);

        _ = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", new AddEventRequest(PlantActionType.Watering, executedAt));

        var response = await this.client.GetAsync("/api/plants?searchText=no-interval&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result![0].LastExecutions[PlantActionType.Watering], Is.EqualTo(executedAt));
            Assert.That(result![0].NextDates, Is.Empty);
        }
    }

    [Test]
    public async Task SearchPlants_WhenSpecieHasInterval_ShouldReturnNextDateBasedOnLastExecution()
    {
        var specieId = await this.CreateSpecieAsync();
        var plantId = await this.CreatePlantAsync("with-interval", specieId);
        var executedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);

        _ = await this.client.PostAsJsonAsync($"/api/species/{specieId}/interval/{PlantActionType.Watering}", new UpdateIntervalRequest(TimeSpan.FromDays(7)));
        _ = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", new AddEventRequest(PlantActionType.Watering, executedAt));

        var response = await this.client.GetAsync("/api/plants?searchText=with-interval&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        Assert.That(result![0].NextDates[PlantActionType.Watering], Is.EqualTo(new DateOnly(2026, 7, 27)));
    }

    [Test]
    public async Task SearchPlants_WhenPlantHasManyEventsOfSameType_ShouldReturnLatestExecution()
    {
        var specieId = await this.CreateSpecieAsync();
        var plantId = await this.CreatePlantAsync("many-events", specieId);
        var older = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc);
        var latest = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);

        _ = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", new AddEventRequest(PlantActionType.Watering, latest));
        _ = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", new AddEventRequest(PlantActionType.Watering, older));

        var response = await this.client.GetAsync("/api/plants?searchText=many-events&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        Assert.That(result![0].LastExecutions[PlantActionType.Watering], Is.EqualTo(latest));
    }

    [Test]
    public async Task SearchPlants_WhenActionTypesDiffer_ShouldReturnLastExecutionPerActionType()
    {
        var specieId = await this.CreateSpecieAsync();
        var plantId = await this.CreatePlantAsync("both-types", specieId);
        var watering = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
        var fertilizing = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc);

        _ = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", new AddEventRequest(PlantActionType.Watering, watering));
        _ = await this.client.PostAsJsonAsync($"/api/plants/{plantId}/events", new AddEventRequest(PlantActionType.Fertilizing, fertilizing));

        var response = await this.client.GetAsync("/api/plants?searchText=both-types&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result![0].LastExecutions[PlantActionType.Watering], Is.EqualTo(watering));
            Assert.That(result![0].LastExecutions[PlantActionType.Fertilizing], Is.EqualTo(fertilizing));
        }
    }

    [Test]
    public async Task SearchPlants_WhenPlantBelongsToGroups_ShouldReturnPlantGroupIds()
    {
        var plantId = await this.CreatePlantAsync("grouped");
        var regionId = await this.CreatePlantGroupAsync(type: GroupType.Region);
        var workGroupId = await this.CreatePlantGroupAsync(type: GroupType.WorkGroup);

        _ = await this.client.PostAsync($"/api/plants/{plantId}/groups/{regionId}", null);
        _ = await this.client.PostAsync($"/api/plants/{plantId}/groups/{workGroupId}", null);

        var response = await this.client.GetAsync("/api/plants?searchText=grouped&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        Assert.That(result![0].PlantGroupIds, Is.EquivalentTo(new[] { regionId, workGroupId }));
    }

    [Test]
    public async Task SearchPlants_WhenPlantWasRemovedFromGroup_ShouldNotReturnPlantGroupId()
    {
        var plantId = await this.CreatePlantAsync("ungrouped");
        var groupId = await this.CreatePlantGroupAsync();

        _ = await this.client.PostAsync($"/api/plants/{plantId}/groups/{groupId}", null);
        _ = await this.client.DeleteAsync($"/api/plants/{plantId}/groups/{groupId}");

        var response = await this.client.GetAsync("/api/plants?searchText=ungrouped&from=0&limit=20");
        var result = await response.Content.ReadFromJsonAsync<PlantDto[]>();

        Assert.That(result![0].PlantGroupIds, Is.Empty);
    }
}

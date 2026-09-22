using Grow.Domain.Plants;
using Grow.WebApi.Dtos;
using Grow.WebApi.Endpoints;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration.Endpoints;

[TestFixture]
public class PlantGroupsTests : IntegrationTestBase
{
    [Test]
    public async Task GetPlantGroups_WhenNoGroupsExist_ShouldReturnEmptyList()
    {
        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPlantGroups_ShouldReturnAllCreatedGroups()
    {
        var firstName = $"group-{Guid.NewGuid()}";
        var secondName = $"group-{Guid.NewGuid()}";
        var firstId = await this.CreatePlantGroupAsync(firstName);
        var secondId = await this.CreatePlantGroupAsync(secondName);

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Select(x => x.Id), Is.EquivalentTo(new[] { firstId, secondId }));
            Assert.That(result!.Select(x => x.Name), Is.EquivalentTo(new[] { firstName, secondName }));
        }
    }

    [Test]
    public async Task GetPlantGroups_ShouldReturnGroupType()
    {
        var regionId = await this.CreatePlantGroupAsync($"region-{Guid.NewGuid()}", GroupType.Region);
        var temporaryId = await this.CreatePlantGroupAsync($"temporary-{Guid.NewGuid()}", GroupType.TemporaryGroup);

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Single(x => x.Id == regionId).Type, Is.EqualTo(GroupType.Region));
            Assert.That(result!.Single(x => x.Id == temporaryId).Type, Is.EqualTo(GroupType.TemporaryGroup));
        }
    }

    [Test]
    public async Task GetPlantGroups_WhenGroupHasMembers_ShouldReturnPlantIds()
    {
        var groupId = await this.CreatePlantGroupAsync();
        var firstPlantId = await this.CreatePlantAsync();
        var secondPlantId = await this.CreatePlantAsync();

        _ = await this.client.PostAsync($"/api/plants/{firstPlantId}/groups/{groupId}", null);
        _ = await this.client.PostAsync($"/api/plants/{secondPlantId}/groups/{groupId}", null);

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        Assert.That(result!.Single().PlantIds, Is.EquivalentTo(new[] { firstPlantId, secondPlantId }));
    }

    [Test]
    public async Task GetPlantGroups_WhenPlantWasRemovedFromGroup_ShouldNotReturnPlantId()
    {
        var groupId = await this.CreatePlantGroupAsync();
        var plantId = await this.CreatePlantAsync();

        _ = await this.client.PostAsync($"/api/plants/{plantId}/groups/{groupId}", null);
        _ = await this.client.DeleteAsync($"/api/plants/{plantId}/groups/{groupId}");

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        Assert.That(result!.Single().PlantIds, Is.Empty);
    }

    [Test]
    public async Task GetPlantGroups_WhenPlantWasAddedTwice_ShouldReturnPlantIdOnce()
    {
        var groupId = await this.CreatePlantGroupAsync();
        var plantId = await this.CreatePlantAsync();

        _ = await this.client.PostAsync($"/api/plants/{plantId}/groups/{groupId}", null);
        var secondResponse = await this.client.PostAsync($"/api/plants/{plantId}/groups/{groupId}", null);

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Single().PlantIds, Is.EquivalentTo(new[] { plantId }));
        }
    }

    [Test]
    public async Task GetPlantGroups_WhenSearchNameIsProvided_ShouldReturnOnlyMatchingGroups()
    {
        var matchingName = $"balcony-{Guid.NewGuid()}";
        var matchingId = await this.CreatePlantGroupAsync(matchingName);
        _ = await this.CreatePlantGroupAsync($"greenhouse-{Guid.NewGuid()}");

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10&searchName=balcony");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Select(x => x.Id), Is.EquivalentTo(new[] { matchingId }));
        }
    }

    [Test]
    public async Task GetPlantGroups_WhenLimitIsLessThanTotalGroups_ShouldReturnLimitedList()
    {
        _ = await this.CreatePlantGroupAsync();
        _ = await this.CreatePlantGroupAsync();

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=1");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task GetPlantGroups_WhenLimitIsBelowAllowedRange_ShouldReturnBadRequest()
    {
        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=0");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetPlantGroups_WhenLimitIsAboveAllowedRange_ShouldReturnBadRequest()
    {
        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=101");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetPlantGroups_ShouldNotReturnGroupsOwnedByAnotherUser()
    {
        var (otherClient, _) = await this.CreateOtherUserClientAsync();

        var otherUsersGroupResponse = await otherClient.PostAsJsonAsync(
            "/api/plant-groups",
            new CreatePlantGroupRequest($"secret-{Guid.NewGuid()}", GroupType.Region));

        Assert.That(otherUsersGroupResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var response = await this.client.GetAsync("/api/plant-groups?from=0&limit=10");
        var result = await response.Content.ReadFromJsonAsync<PlantGroupDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Empty);
        }
    }
}

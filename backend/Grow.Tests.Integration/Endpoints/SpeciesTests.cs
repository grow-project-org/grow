using Grow.WebApi.Dtos;
using Grow.WebApi.Endpoints;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration.Endpoints;

[TestFixture]
public class SpeciesTests : IntegrationTestBase
{
    [Test]
    public async Task GetSpecies_WhenNoSpeciesExist_ShouldReturnEmptyList()
    {
        var response = await this.client.GetAsync("/api/species");
        var result = await response.Content.ReadFromJsonAsync<SpecieDto[]>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetSpecies_ShouldReturnAllCreatedSpecies()
    {
        var firstName = $"monstera-{Guid.NewGuid()}";
        var secondName = $"fern-{Guid.NewGuid()}";
        var firstId = await this.CreateSpecieAsync(firstName);
        var secondId = await this.CreateSpecieAsync(secondName);

        var response = await this.client.GetAsync("/api/species");
        var result = await response.Content.ReadFromJsonAsync<SpecieDto[]>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Select(s => s.Id), Is.EquivalentTo([firstId, secondId]));
        Assert.That(result!.Select(s => s.Name), Is.EquivalentTo([firstName, secondName]));
    }

    [Test]
    public async Task CreateSpecie_ShouldReturnSpecieId()
    {
        var newSpecieRequest = new CreateSpecieRequest($"monstera-{Guid.NewGuid()}");

        var response = await this.client.PostAsJsonAsync("/api/species", newSpecieRequest);
        var result = await response.Content.ReadFromJsonAsync<CreateSpecieResponse>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        Assert.That(result!.SpecieId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CreateSpecie_ShouldPersistSpecieWithGivenName()
    {
        var name = $"monstera-{Guid.NewGuid()}";
        var newSpecieRequest = new CreateSpecieRequest(name);

        var response = await this.client.PostAsJsonAsync("/api/species", newSpecieRequest);
        var result = await response.Content.ReadFromJsonAsync<CreateSpecieResponse>();

        await using var db = await this.factory.CreateDbContextAsync();
        var specie = await db.Species.FindAsync(result!.SpecieId);

        Assert.That(specie, Is.Not.Null);
        Assert.That(specie!.Name, Is.EqualTo(name));
    }
}

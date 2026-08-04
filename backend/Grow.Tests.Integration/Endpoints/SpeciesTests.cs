using Grow.WebApi.Endpoints;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration.Endpoints;

[TestFixture]
public class SpeciesTests : IntegrationTestBase
{
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
